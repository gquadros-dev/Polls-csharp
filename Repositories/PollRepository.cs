using Microsoft.EntityFrameworkCore;
using POLLS.Data;
using POLLS.Interfaces;
using POLLS.Models;
using StackExchange.Redis;

namespace POLLS.Repositories
{
    public class PollRepository : IPollRepository
    {
        private readonly PollsDbContext _dbContext;
        private readonly IConnectionMultiplexer _redis;

        public PollRepository(PollsDbContext dbContext, IConnectionMultiplexer redis)
        {
            _dbContext = dbContext;
            _redis = redis;
        }

        public async Task<Poll> CreateAsync(Poll poll)
        {
            _dbContext.Polls.Add(poll);
            await _dbContext.SaveChangesAsync();
            return poll;
        }

        public async Task<Poll?> GetByIdAsync(Guid pollId)
        {
            return await _dbContext.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId);
        }

        public async Task<Dictionary<string, int>> GetPollResultsAsync(string pollId)
        {
            var redisDb = _redis.GetDatabase();
            var redisResult = await redisDb.SortedSetRangeByRankWithScoresAsync(pollId);

            return redisResult.ToDictionary(
                entry => entry.Element.ToString(),
                entry => (int)entry.Score
            );
        }
    }
}