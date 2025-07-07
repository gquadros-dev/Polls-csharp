using Microsoft.AspNetCore.Mvc;
using POLLS.DTo.Requests;
using POLLS.Interfaces;
using POLLS.Models;

namespace POLLS.Services
{
    public class VoteService : IVoteService
    {
        private readonly IVoteRepository _voteRepository;
        private readonly INotificationService _notificationService;

        public VoteService(IVoteRepository voteRepository, INotificationService notificationService)
        {
            _voteRepository = voteRepository;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> VoteOnPollAsync(Guid pollId, VoteOnPollRequest request, string? sessionId, Action<string> setSessionIdCookie)
        {
            var pollOption = await _voteRepository.GetPollOptionByIdAsync(request.PollOptionId, pollId);
            if (pollOption is null)
            {
                return new NotFoundObjectResult(new { message = "Poll option not found." });
            }

            if (sessionId is not null)
            {
                var userPreviousVote = await _voteRepository.GetUserVoteAsync(sessionId, pollId);
                if (userPreviousVote is not null)
                {
                    if (userPreviousVote.PollOptionId == request.PollOptionId)
                    {
                        return new ConflictObjectResult(new { message = "You have already voted on this poll." });
                    }
                    _voteRepository.RemoveVote(userPreviousVote);
                }
            }

            if (sessionId is null)
            {
                sessionId = Guid.NewGuid().ToString();
                setSessionIdCookie(sessionId);
            }

            _voteRepository.AddVote(new Vote
            {
                SessionId = sessionId,
                PollId = pollId,
                PollOptionId = request.PollOptionId
            });

            await _voteRepository.SaveChangesAsync();

            var voteCount = await _voteRepository.IncrementVoteCountAsync(pollId.ToString(), request.PollOptionId.ToString());
            await _notificationService.PublishVoteNotificationAsync(pollId, request.PollOptionId, voteCount);

            return new CreatedResult();
        }
    }
}