using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POLLS.Data;
using POLLS.DTo.Requests;
using POLLS.Messaging;
using POLLS.Models;
using StackExchange.Redis;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace POLLS.Controllers
{
    [ApiController]
    [Route("polls/{pollId}")]
    public class VotesController : ControllerBase
    {
        private readonly PollsDbContext _dbContext;
        private readonly IConnectionMultiplexer _redis;

        public VotesController(PollsDbContext dbContext, IConnectionMultiplexer redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        [HttpPost("votes")]
        public async Task<IActionResult> VoteOnPoll(Guid pollId, [FromBody] VoteOnPollRequest request)
        {
            var pollOption = await _dbContext.PollOptions
                .FirstOrDefaultAsync(o => o.Id == request.PollOptionId && o.PollId == pollId);

            if (pollOption is null)
            {
                return NotFound(new { message = "Poll option not found." });
            }

            HttpContext.Request.Cookies.TryGetValue("sessionId", out var sessionId);

            if (sessionId is not null)
            {
                var userPreviousVote = await _dbContext.Votes
                    .FirstOrDefaultAsync(v => v.SessionId == sessionId && v.PollId == pollId);

                if (userPreviousVote is not null)
                {
                    if (userPreviousVote.PollOptionId == request.PollOptionId)
                    {
                        return Conflict(new { message = "You have already voted on this poll." });
                    }

                    _dbContext.Votes.Remove(userPreviousVote);
                }
            }

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
                HttpContext.Response.Cookies.Append("sessionId", sessionId, cookieOptions);
            }

            _dbContext.Votes.Add(new Vote
            {
                SessionId = sessionId,
                PollId = pollId,
                PollOptionId = request.PollOptionId
            });

            await _dbContext.SaveChangesAsync();

            var redisDb = _redis.GetDatabase();
            var subscriber = _redis.GetSubscriber();

            var votes = await redisDb.SortedSetIncrementAsync(pollId.ToString(), request.PollOptionId.ToString(), 1);

            var finalNotification = new VoteNotification { PollOptionId = request.PollOptionId, Votes = (int)votes };
            await subscriber.PublishAsync(RedisChannel.Literal(pollId.ToString()), JsonSerializer.Serialize(finalNotification));

            return Created();
        }

        [HttpGet("results")]
        public async Task GetPollResults(Guid pollId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var subscriber = _redis.GetSubscriber();

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
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsync("This endpoint requires a WebSocket connection");
            }
        }
    }
}