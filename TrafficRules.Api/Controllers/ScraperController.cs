using Microsoft.AspNetCore.Mvc;
using TrafficRules.Api.Services;
using TrafficRules.Domain.Entities;

namespace TrafficRules.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly TelegramBotService _telegramBotService;
    private readonly ILogger<ScraperController> _logger;
    private readonly IWebHostEnvironment _env;

    public ScraperController(
        IConfiguration configuration, 
        TelegramBotService telegramBotService,
        ILogger<ScraperController> logger,
        IWebHostEnvironment env)
    {
        _configuration = configuration;
        _telegramBotService = telegramBotService;
        _logger = logger;
        _env = env;
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportQuestion([FromBody] ScraperPayload payload)
    {
        var secretKey = _configuration["ScraperSecretKey"];
        if (payload.SecretKey != secretKey)
        {
            _logger.LogWarning("Scraper unauthorized access attempt.");
            return Unauthorized("Invalid Secret Key");
        }

        string? imageUrl = null;
        if (!string.IsNullOrEmpty(payload.ImageBase64))
        {
            try
            {
                var bytes = Convert.FromBase64String(payload.ImageBase64);
                var imagesPath = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);

                var fileName = $"q_scraped_{Guid.NewGuid():N}.jpg";
                var filePath = Path.Combine(imagesPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                imageUrl = $"/images/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save scraped image.");
            }
        }

        var draft = new QuestionDraft
        {
            CategoryId = payload.CategoryId,
            Type = payload.Type,
            Text = payload.Text,
            ImageUrl = imageUrl,
            Explanation = payload.Explanation,
            Answers = payload.Answers ?? new List<string>(),
            CorrectAnswerIndices = payload.CorrectAnswerIndices ?? new List<int>(),
            MatchingPairs = payload.MatchingPairs ?? new List<string>(),
            SequenceItems = payload.SequenceItems ?? new List<string>(),
            CorrectTextAnswer = payload.CorrectTextAnswer
        };

        await _telegramBotService.SendDraftToAdminAsync(draft);
        return Ok(new { message = "Draft sent to Telegram for review." });
    }
}

public class ScraperPayload
{
    public string SecretKey { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string? ImageBase64 { get; set; }

    public List<string>? Answers { get; set; }
    public List<int>? CorrectAnswerIndices { get; set; }
    public List<string>? MatchingPairs { get; set; }
    public List<string>? SequenceItems { get; set; }
    public string? CorrectTextAnswer { get; set; }
}
