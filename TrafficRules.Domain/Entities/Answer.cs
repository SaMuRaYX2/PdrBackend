namespace TrafficRules.Domain.Entities;

public class Answer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Text { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; }
    
    public int SortOrder { get; set; }
    
    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;
}