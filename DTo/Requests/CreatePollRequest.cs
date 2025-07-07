using System.ComponentModel.DataAnnotations;

namespace POLLS.Http.Requests;

public class CreatePollRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(2, ErrorMessage = "A enquete precisa de pelo menos duas opções.")]
    public List<string> Options { get; set; } = new List<string>();
}