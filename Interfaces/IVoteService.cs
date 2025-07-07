using POLLS.DTo.Requests;
using Microsoft.AspNetCore.Mvc;

namespace POLLS.Interfaces
{
    public interface IVoteService
    {
        Task<IActionResult> VoteOnPollAsync(Guid pollId, VoteOnPollRequest request, string? sessionId, Action<string> setSessionIdCookie);
    }
}