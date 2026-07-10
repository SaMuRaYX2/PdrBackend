using TrafficRules.Domain.Entities;

namespace TrafficRules.Application.DTOs;

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    // Тип питання (0=SingleChoice, 1=MultipleChoice, 2=TrueFalse...)
    public QuestionType Type { get; set; }
    
    // Чи є декілька правильних відповідей (для зручності на фронтенді)
    public bool IsMultipleChoice { get; set; }

    public List<AnswerDto> Answers { get; set; } = new();
}
