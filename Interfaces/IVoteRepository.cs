using POLLS.Models;

namespace POLLS.Interfaces
{
    public interface IVoteRepository
    {
        Task<Vote?> GetUserVoteAsync(string sessionId, Guid pollId);
        Task<PollOption?> GetPollOptionByIdAsync(Guid pollOptionId, Guid pollId);
        void RemoveVote(Vote vote);
        void AddVote(Vote vote);
        Task<int> SaveChangesAsync();
        Task<double> IncrementVoteCountAsync(string pollId, string pollOptionId);
    }
}