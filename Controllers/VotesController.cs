using Microsoft.AspNetCore.Mvc;
using POLLS.DTo.Requests;
using POLLS.Interfaces;

namespace POLLS.Controllers
{
    [ApiController]
    [Route("polls/{pollId}")]
    public class VotesController : ControllerBase
    {
        private readonly IVoteService _voteService;
        private readonly INotificationService _notificationService;

        public VotesController(IVoteService voteService, INotificationService notificationService)
        {
            _voteService = voteService;
            _notificationService = notificationService;
        }

        [HttpPost("votes")]
        public async Task<IActionResult> VoteOnPoll(Guid pollId, [FromBody] VoteOnPollRequest request)
        {
            HttpContext.Request.Cookies.TryGetValue("sessionId", out var sessionId);

            Action<string> setSessionIdCookie = (newSessionId) =>
            {
                var cookieOptions = new CookieOptions
                {
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(30),
                    IsEssential = true,
                    HttpOnly = true,
                };
                HttpContext.Response.Cookies.Append("sessionId", newSessionId, cookieOptions);
            };

            return await _voteService.VoteOnPollAsync(pollId, request, sessionId, setSessionIdCookie);
        }

        [HttpGet("results")]
        public async Task GetPollResults(Guid pollId)
        {
            await _notificationService.HandleWebSocketConnectionAsync(HttpContext, pollId);
        }
    }
}