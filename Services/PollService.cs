using System.Security.Cryptography.X509Certificates;
using POLLS.DTo.Requests;
using POLLS.DTo.Responses;
using POLLS.Interfaces;
using POLLS.Models;

namespace POLLS.Services
{
    public class PollService : IPollService
    {
        private readonly IPollRepository _pollRepository;

        public PollService(IPollRepository pollRepository)
        {
            _pollRepository = pollRepository;
        }

        public async Task<PollResponse> CreatePollAsync(CreatePollRequest request)
        {
            var poll = new Poll
            {
                Title = request.Title,
                Options = request.Options.Select(optionTitle => new PollOption { Title = optionTitle }).ToList()
            };

            var createdPoll = await _pollRepository.CreateAsync(poll);

            return new PollResponse { PollId = createdPoll.Id };
        }

        public async Task<GetPollResponse?> GetPollAsync(Guid pollId)
        {
            var poll = await _pollRepository.GetByIdAsync(pollId);

            if (poll is null)
            {
                return null;
            }

            var votes = await _pollRepository.GetPollResultsAsync(pollId.ToString());

            var response = new GetPollResponse
            {
                Id = poll.Id,
                Title = poll.Title,
                Options = poll.Options.Select(option => new PollOptionResponse
                {
                    Id = option.Id,
                    Title = option.Title,
                    Score = votes.GetValueOrDefault(option.Id.ToString(), 0)
                }).ToList()
            };

            return response;
        }
    }
}