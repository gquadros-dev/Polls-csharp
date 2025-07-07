using Microsoft.EntityFrameworkCore;
using POLLS.Data;
using POLLS.Interfaces;
using POLLS.Models;
using StackExchange.Redis;

namespace POLLS.Repositories
{
    public class VoteRepository : IVoteRepository
    {
        private readonly PollsDbContext _dbContext;
        private readonly IConnectionMultiplexer _redis;

        public VoteRepository(PollsDbContext dbContext, IConnectionMultiplexer redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        public async Task<PollOption?> GetPollOptionByIdAsync(Guid pollOptionId, Guid pollId)
        {
            return await _dbContext.PollOptions
                .FirstOrDefaultAsync(o => o.Id == pollOptionId && o.PollId == pollId);
        }

        public async Task<Vote?> GetUserVoteAsync(string sessionId, Guid pollId)
        {
            return await _dbContext.Votes
                .FirstOrDefaultAsync(v => v.SessionId == sessionId && v.PollId == pollId);
        }

        public void AddVote(Vote vote)
        {
            _dbContext.Votes.Add(vote);
        }

        public void RemoveVote(Vote vote)
        {
            _dbContext.Votes.Remove(vote);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<double> IncrementVoteCountAsync(string pollId, string pollOptionId)
        {
            var redisDb = _redis.GetDatabase();
            return await redisDb.SortedSetIncrementAsync(pollId, pollOptionId, 1);
        }
    }
}