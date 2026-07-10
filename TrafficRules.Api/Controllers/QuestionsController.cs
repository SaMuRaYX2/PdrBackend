using Microsoft.AspNetCore.Mvc;
using TrafficRules.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace TrafficRules.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionsService _questionsService;

    public QuestionsController(IQuestionsService questionsService)
    {
        _questionsService = questionsService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllQuestions()
    {
        var questions = await _questionsService.GetAllQuestionsAsync();
        return Ok(questions);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetQuestionById(Guid id)
    {
        var question = await _questionsService.GetQuestionByIdAsync(id);
        if (question == null) return NotFound();
        return Ok(question);
    }

    [Authorize]
    // SingleChoice / TrueFalse
    [HttpPost("{id:guid}/check")]
    public async Task<IActionResult> CheckAnswer(Guid id, [FromBody] Guid answerId)
    {
        var result = await _questionsService.CheckAnswerAsync(id, answerId);
        return Ok(result);
    }

    [Authorize]
    // MultipleChoice
    [HttpPost("{id:guid}/check-multiple")]
    public async Task<IActionResult> CheckMultipleAnswers(Guid id, [FromBody] List<Guid> answerIds)
    {
        var result = await _questionsService.CheckMultipleAnswersAsync(id, answerIds);
        return Ok(result);
    }

    [Authorize]
    // Sequence
    [HttpPost("{id:guid}/check-sequence")]
    public async Task<IActionResult> CheckSequence(Guid id, [FromBody] List<Guid> orderedAnswerIds)
    {
        var result = await _questionsService.CheckSequenceAsync(id, orderedAnswerIds);
        return Ok(result);
    }

    [Authorize]
    // NumberInput / ShortAnswer / Matching
    [HttpPost("{id:guid}/check-text")]
    public async Task<IActionResult> CheckTextAnswer(Guid id, [FromBody] string userAnswer)
    {
        var result = await _questionsService.CheckTextAnswerAsync(id, userAnswer);
        return Ok(result);
    }
}