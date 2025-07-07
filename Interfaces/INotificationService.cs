namespace POLLS.Interfaces
{
    public interface INotificationService
    {
        Task PublishVoteNotificationAsync(Guid pollId, Guid pollOptionId, double voteCount);
        Task HandleWebSocketConnectionAsync(HttpContext context, Guid pollId);
    }
}