namespace TrafficRules.Domain.Entities;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Text { get; set; } = string.Empty;
    
    public string? ImageUrl { get; set; }
    
    public string? Explanation { get; set; }
    
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public bool HasMultipleCorrectAnswers()
    {
        return Answers.Count(a => a.IsCorrect) > 1;
    }
    
    public Guid CategoryId { get; set; }
    
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;

    public string? PdrRuleReference { get; set; }
    public Category Category { get; set; } = null!;
}