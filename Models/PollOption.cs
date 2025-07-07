namespace POLLS.Models;

public class PollOption
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

    // Chave estrangeira para a Poll
    public Guid PollId { get; set; }

    // Relações (navigation properties)
    public Poll? Poll { get; set; }
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}