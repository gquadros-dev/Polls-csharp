namespace POLLS.Models;

public class Vote
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Chaves estrangeiras 
    public Guid PollOptionId { get; set; }
    public Guid PollId { get; set; }

    // Relações 
    public PollOption? PollOption { get; set; }
    public Poll? Poll { get; set; }
}