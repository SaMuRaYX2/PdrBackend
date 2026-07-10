using TrafficRules.Application.DTOs;

namespace TrafficRules.Application.Interfaces;

public interface IQuestionsService
{
    Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync();
    Task<QuestionDto?> GetQuestionByIdAsync(Guid id);
    Task<AnswerResultDto> CheckAnswerAsync(Guid questionId, Guid answerId);
    Task<AnswerResultDto> CheckMultipleAnswersAsync(Guid questionId, List<Guid> answerIds);
    // NEW: For Sequence type - check if order of answer IDs is correct
    Task<AnswerResultDto> CheckSequenceAsync(Guid questionId, List<Guid> orderedAnswerIds);
    // NEW: For NumberInput and ShortAnswer types - check text answer
    Task<AnswerResultDto> CheckTextAnswerAsync(Guid questionId, string userAnswer);
    
}