namespace TrafficRules.Application.DTOs;

public class AnswerDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    // For Sequence type - position in correct order (1-based)
    public int SortOrder { get; set; }
}