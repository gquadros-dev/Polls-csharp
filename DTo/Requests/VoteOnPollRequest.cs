using System.ComponentModel.DataAnnotations;

namespace POLLS.Http.Requests;

public class VoteOnPollRequest
{
    [Required]
    public Guid PollOptionId { get; set; }
}