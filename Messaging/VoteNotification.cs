namespace POLLS.Messaging;

public class VoteNotification
{
    public Guid PollOptionId { get; set; }
    public int Votes { get; set; }
}