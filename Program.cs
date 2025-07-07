using POLLS.Data;
using POLLS.Models;
using POLLS.Messaging;
using POLLS.Http.Requests;
using POLLS.Http.Responses;

using System.Text;
using System.Text.Json;
using System.Net.WebSockets;

using StackExchange.Redis;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6380")
);

builder.Services.AddDbContext<PollsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

app.UseStaticFiles();
app.UseWebSockets();

app.MapPost("/polls", async (CreatePollRequest request, PollsDbContext dbContext) =>
{
    var poll = new Poll
    {
        Title = request.Title,
        Options = request.Options.Select(optionTitle => new PollOption { Title = optionTitle }).ToList()
    };

    dbContext.Polls.Add(poll);

    await dbContext.SaveChangesAsync();

    return Results.Created($"/polls/{poll.Id}", new {pollId = poll.Id});
});

app.MapGet("/polls/{pollId}", async (Guid pollId, PollsDbContext dbContext, IConnectionMultiplexer redis) =>
{
    // Busca a enquete lá no banco Postgres
    var poll = await dbContext.Polls
        .Where(p => p.Id == pollId)
        .Select(p => new
        {
            p.Id,
            p.Title,
            Options = p.Options.Select(o => new { o.Id, o.Title }).ToList()
        })
        .FirstOrDefaultAsync();

    // Se a enquete não for encontrada, retorna 404
    if (poll is null)
    {
        return Results.NotFound(new { message = "Poll not found!" });
    }

    // Busca os votos no Redis
    var redisDb = redis.GetDatabase();
    var redisResult = await redisDb.SortedSetRangeByRankWithScoresAsync(pollId.ToString());

    // Transforma o resultado do Redis em um dicionário
    var votes = redisResult
        .ToDictionary(
            entry => entry.Element.ToString(),
            entry => (int)entry.Score
        );

    var response = new GetPollResponse
    {
        Id = poll.Id,
        Title = poll.Title,
        Options = poll.Options.Select(option => new PollOptionResponse
        {
            Id = option.Id,
            Title = option.Title,
            Score = votes.GetValueOrDefault(option.Id.ToString(), 0)
        }).ToList()
    };

    return Results.Ok(response);
});

app.MapPost("/polls/{pollId}/votes", async (
    Guid pollId,
    VoteOnPollRequest request,
    PollsDbContext dbContext,
    IConnectionMultiplexer redis,
    HttpContext httpContext) =>
{
    var pollOption = await dbContext.PollOptions
        .FirstOrDefaultAsync(o => o.Id == request.PollOptionId && o.PollId == pollId);

    if (pollOption is null)
    {
        return Results.NotFound(new { message = "Poll option not found." });
    }

    httpContext.Request.Cookies.TryGetValue("sessionId", out var sessionId);

    // Se o usuário já tem uma sessão
    if (sessionId is not null)
    {
        var userPreviousVote = await dbContext.Votes
            .FirstOrDefaultAsync(v => v.SessionId == sessionId && v.PollId == pollId);

        if (userPreviousVote is not null)
        {
            if (userPreviousVote.PollOptionId == request.PollOptionId)
            {
                return Results.Conflict(new { message = "You have already voted on this poll." });
            }

            // Se o voto for diferente, apenas deleta o voto antigo do CONTEXTO.
            // A deleção só será efetivada no banco com o SaveChangesAsync().
            dbContext.Votes.Remove(userPreviousVote);
        }
    }
    
    // Se não tinha uma sessão, cria uma nova
    if (sessionId is null)
    {
        sessionId = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(30),
            IsEssential = true,
            HttpOnly = true,
        };
        httpContext.Response.Cookies.Append("sessionId", sessionId, cookieOptions);
    }
    
    dbContext.Votes.Add(new Vote
    {
        SessionId = sessionId,
        PollId = pollId,
        PollOptionId = request.PollOptionId
    });

    await dbContext.SaveChangesAsync();

    var redisDb = redis.GetDatabase();
    var subscriber = redis.GetSubscriber();

    var votes = await redisDb.SortedSetIncrementAsync(pollId.ToString(), request.PollOptionId.ToString(), 1);
    
    var finalNotification = new VoteNotification { PollOptionId = request.PollOptionId, Votes = (int)votes };
    await subscriber.PublishAsync(RedisChannel.Literal(pollId.ToString()), JsonSerializer.Serialize(finalNotification));

    return Results.Created();
});

app.MapGet("/polls/{pollId}/results", async (Guid pollId, HttpContext context, IConnectionMultiplexer redis) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        // Se for uma requisição WebSocket, aceita a conexão
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var subscriber = redis.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal(pollId.ToString()), async (channel, message) =>
        {
            var data = Encoding.UTF8.GetBytes(message.ToString());
            await webSocket.SendAsync(
                new ReadOnlyMemory<byte>(data),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        });

        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
        }

        await subscriber.UnsubscribeAsync(RedisChannel.Literal(pollId.ToString()));
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("This endpoint requires a WebSocket connection");
    }
});

Console.WriteLine("HTTP Server running! Verifique as URLs no seu terminal.");
app.Run();
