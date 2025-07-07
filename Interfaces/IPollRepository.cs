using POLLS.Models;

namespace POLLS.Interfaces
{
    public interface IPollRepository
    {
        Task<Poll> CreateAsync(Poll poll);
        Task<Poll?> GetByIdAsync(Guid pollId);
        Task<Dictionary<string, int>> GetPollResultsAsync(string pollId);
    }
}