using POLLS.DTo.Requests;
using POLLS.DTo.Responses;

namespace POLLS.Interfaces
{
    public interface IPollService
    {
        Task<PollResponse> CreatePollAsync(CreatePollRequest request);
        Task<GetPollResponse?> GetPollAsync(Guid pollId);
    }
}