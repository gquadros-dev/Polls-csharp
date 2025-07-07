using Microsoft.AspNetCore.Mvc;
using POLLS.DTo.Requests;
using POLLS.Interfaces;

namespace POLLS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PollsController : ControllerBase
    {
        private readonly IPollService _pollService;

        public PollsController(IPollService pollService)
        {
            _pollService = pollService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
        {
            var pollResponse = await _pollService.CreatePollAsync(request);
            return Created($"/polls/{pollResponse.PollId}", pollResponse);
        }

        [HttpGet("{pollId}")]
        public async Task<IActionResult> GetPoll(Guid pollId)
        {
            var pollResponse = await _pollService.GetPollAsync(pollId);

            if (pollResponse is null)
            {
                return NotFound(new { messaage = "Poll not found! " });
            }

            return Ok(pollResponse);
        }
    }
}