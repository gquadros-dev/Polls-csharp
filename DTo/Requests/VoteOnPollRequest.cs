using System.ComponentModel.DataAnnotations;

namespace POLLS.DTo.Requests;

public class VoteOnPollRequest
{
    [Required]
    public Guid PollOptionId { get; set; }
}