namespace TrafficRules.Application.DTOs;

public class AnswerResultDto
{
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set;}
    
    public Guid CorrectAnswerId { get; set; }

    public List<Guid> CorrectAnswerIds { get; set; } = new();
}