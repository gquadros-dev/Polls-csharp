using POLLS.Interfaces;
using POLLS.Messaging;
using StackExchange.Redis;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace POLLS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConnectionMultiplexer _redis;

        public NotificationService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task PublishVoteNotificationAsync(Guid pollId, Guid pollOptionId, double voteCount)
        {
            var subscriber = _redis.GetSubscriber();
            var notification = new VoteNotification { PollOptionId = pollOptionId, Votes = (int)voteCount };
            var channel = RedisChannel.Literal(pollId.ToString());
            var messagePayload = JsonSerializer.Serialize(notification);

            await subscriber.PublishAsync(channel, messagePayload);
        }

        public async Task HandleWebSocketConnectionAsync(HttpContext context, Guid pollId)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("This endpoint requires a WebSocket connection");
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var subscriber = _redis.GetSubscriber();
            var channel = RedisChannel.Literal(pollId.ToString());

            await subscriber.SubscribeAsync(channel, async (_, message) =>
            {
                var data = Encoding.UTF8.GetBytes(message.ToString());
                await webSocket.SendAsync(new ReadOnlyMemory<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
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

            await subscriber.UnsubscribeAsync(channel);
        }
    }
}