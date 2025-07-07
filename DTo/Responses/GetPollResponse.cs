namespace POLLS.Http.Responses;

public class PollOptionResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class GetPollResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<PollOptionResponse> Options { get; set; } = [];
}