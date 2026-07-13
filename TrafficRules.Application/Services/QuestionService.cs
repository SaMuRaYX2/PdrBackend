using TrafficRules.Application.DTOs;
using TrafficRules.Application.Interfaces;
using TrafficRules.Domain.Entities;
using TrafficRules.Domain.Interfaces;

namespace TrafficRules.Application.Services;

public class QuestionService : IQuestionsService
{
    private readonly IQuestionRepository _repository;

    public QuestionService(IQuestionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync()
    {
        var questions = await _repository.GetAllAsync();
        return questions.Select(MapToDto);
    }

    public async Task<QuestionDto?> GetQuestionByIdAsync(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        return question == null ? null : MapToDto(question);
    }

    public async Task<AnswerResultDto> CheckAnswerAsync(Guid questionId, Guid answerId)
    {
        var question = await _repository.GetByIdAsync(questionId);
        if (question == null) return new AnswerResultDto { IsCorrect = false };

        var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == answerId);
        var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
        var correctIds = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();

        return new AnswerResultDto
        {
            IsCorrect = selectedAnswer != null && selectedAnswer.IsCorrect,
            Explanation = question.Explanation,
            CorrectAnswerId = correctAnswer?.Id ?? Guid.Empty,
            CorrectAnswerIds = correctIds
        };
    }

    public async Task<AnswerResultDto> CheckMultipleAnswersAsync(Guid questionId, List<Guid> answerIds)
    {
        var question = await _repository.GetByIdAsync(questionId);
        if (question == null) return new AnswerResultDto { IsCorrect = false };

        var correctIds = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
        var isCorrect = correctIds.Count == answerIds.Count && correctIds.All(id => answerIds.Contains(id));

        return new AnswerResultDto
        {
            IsCorrect = isCorrect,
            Explanation = question.Explanation,
            CorrectAnswerId = correctIds.FirstOrDefault(),
            CorrectAnswerIds = correctIds
        };
    }

    public async Task<AnswerResultDto> CheckSequenceAsync(Guid questionId, List<Guid> orderedAnswerIds)
    {
        var question = await _repository.GetByIdAsync(questionId);
        if (question == null) return new AnswerResultDto { IsCorrect = false };

        // Correct order is the order they were saved (by SortOrder / index)
        var correctOrder = question.Answers
            .OrderBy(a => a.SortOrder)
            .Select(a => a.Id)
            .ToList();

        var isCorrect = correctOrder.Count == orderedAnswerIds.Count &&
                        correctOrder.SequenceEqual(orderedAnswerIds);

        return new AnswerResultDto
        {
            IsCorrect = isCorrect,
            Explanation = question.Explanation,
            CorrectAnswerIds = correctOrder
        };
    }

    public async Task<AnswerResultDto> CheckTextAnswerAsync(Guid questionId, string userAnswer)
    {
        var question = await _repository.GetByIdAsync(questionId);
        if (question == null) return new AnswerResultDto { IsCorrect = false };

        var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
        if (correctAnswer == null) return new AnswerResultDto { IsCorrect = false };

        // Case-insensitive comparison, trim whitespace
        var isCorrect = string.Equals(
            correctAnswer.Text.Trim(),
            userAnswer.Trim(),
            StringComparison.OrdinalIgnoreCase);

        return new AnswerResultDto
        {
            IsCorrect = isCorrect,
            Explanation = question.Explanation,
            CorrectAnswerId = correctAnswer.Id,
            CorrectAnswerIds = new List<Guid> { correctAnswer.Id }
        };
    }

    public async Task<bool> DeleteQuestionAsync(Guid id)
    {
        var question = await _repository.GetByIdAsync(id);
        if (question == null) return false;
        
        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static QuestionDto MapToDto(Question q)
    {
        return new QuestionDto
        {
            Id = q.Id,
            Text = q.Text,
            ImageUrl = q.ImageUrl,
            Type = q.Type,
            IsMultipleChoice = q.Answers.Count(a => a.IsCorrect) > 1,
            Answers = q.Answers.Select((a, i) => new AnswerDto
            {
                Id = a.Id,
                Text = a.Text,
                SortOrder = a.SortOrder > 0 ? a.SortOrder : (i + 1)
            }).ToList()
        };
    }
}