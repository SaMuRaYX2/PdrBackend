using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TrafficRules.Domain.Entities;
using TrafficRules.Infrastructure.Data;
using TrafficRules.Application.Interfaces;

namespace TrafficRules.Api.Services;

// ======================================================
// СТАНИ ДІАЛОГУ З БОТОМ
// ======================================================
public enum AdminState
{
    Idle,
    WaitingForCategories,
    WaitingForType,
    WaitingForQuestionText,
    WaitingForPhoto,
    // SingleChoice / MultipleChoice
    WaitingForAnswers,
    WaitingForCorrectAnswer,
    // TrueFalse - автоматично створює "Так"/"Ні"
    WaitingForTrueFalseAnswer,
    // Matching - пари "Ліве = Праве"
    WaitingForMatchingPairs,
    // Sequence - елементи у правильному порядку
    WaitingForSequenceItems,
    // NumberInput
    WaitingForCorrectNumber,
    // ShortAnswer
    WaitingForCorrectText,
    // Спільне
    WaitingForExplanation
}

// ======================================================
// ЧЕРНЕТКА ПИТАННЯ
// ======================================================
public class QuestionDraft
{
    public Guid CategoryId { get; set; }
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    // Для SingleChoice / MultipleChoice
    public List<string> Answers { get; set; } = new();
    public List<int> CorrectAnswerIndices { get; set; } = new();
    
    // Для Matching (зберігаємо як "Лівий|||Правий")
    public List<string> MatchingPairs { get; set; } = new();
    
    // Для Sequence (елементи у правильному порядку)
    public List<string> SequenceItems { get; set; } = new();
    
    // Для NumberInput / ShortAnswer
    public string? CorrectTextAnswer { get; set; }
    
    public string? Explanation { get; set; }
}

// ======================================================
// СЕРВІС БОТА
// ======================================================
public class TelegramBotService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IImageStorageService _imageStorage;
    private readonly TelegramBotClient _botClient;

    private static readonly Dictionary<long, AdminState> _userStates = new();
    private static readonly Dictionary<long, QuestionDraft> _drafts = new();

    public TelegramBotService(IConfiguration configuration, ILogger<TelegramBotService> logger,
        IServiceScopeFactory scopeFactory, IImageStorageService imageStorage)
    {
        _configuration = configuration;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _imageStorage = imageStorage;
        var token = _configuration["TelegramBotToken"];
        _botClient = new TelegramBotClient(token!);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var commands = new[]
        {
            new BotCommand { Command = "add", Description = "одати нове питання до БД"},
            new BotCommand { Command = "cancel", Description = "Скасувати поточну дію та повернутись" },
            new BotCommand { Command = "help", Description = "Допомога по роботі з ботом" }
        };
        
        await _botClient.SetMyCommands(commands, cancellationToken: stoppingToken);
        
        
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            cancellationToken: stoppingToken
        );
        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation($"🤖 Бот {{{{me.Username}}}} запущений! Меню команд оновлено.");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (update.Message is not { } message) return;

        var chatId = message.Chat.Id;
        var text = message.Text ?? string.Empty;

        if (!_userStates.ContainsKey(chatId))
        {
            _userStates[chatId] = AdminState.Idle;
            _drafts[chatId] = new QuestionDraft();
        }

        // ── ГЛОБАЛЬНІ КОМАНДИ ──────────────────────────
        if (text == "/start" || text == "/help")
        {
            await botClient.SendMessage(chatId,
                "🚦 *ПДР Адмін Бот*\n\n" +
                "Команди:\n" +
                "/add — додати нове питання\n" +
                "/cancel — скасувати поточну дію",
                parseMode: ParseMode.Markdown, cancellationToken: ct);
            return;
        }

        if (text == "/add")
        {
            _userStates[chatId] = AdminState.WaitingForCategories;
            _drafts[chatId] = new QuestionDraft();
            
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var categories = dbContext.Categories.OrderBy(c => c.Name).ToList();
            
            if (!categories.Any())
            {
                await botClient.SendMessage(chatId, "⚠️ В базі даних немає жодної категорії. Спочатку додайте їх у базу.", cancellationToken: ct);
                return;
            }

            var buttons = categories.Select(c => new[] { new KeyboardButton(c.Name) }).ToArray();
            var keyboard = new ReplyKeyboardMarkup(buttons){ ResizeKeyboard = true, OneTimeKeyboard = true };
            
            await botClient.SendMessage(chatId, "Оберіть *категорію*:", parseMode: ParseMode.Markdown,
                replyMarkup: keyboard, cancellationToken: ct);
            return;
            
        }

        if (text == "/cancel")
        {
            _userStates[chatId] = AdminState.Idle;
            await botClient.SendMessage(chatId, "❌ Скасовано. Введіть /add щоб почати.",
                replyMarkup: new ReplyKeyboardRemove(), cancellationToken: ct);
            return;
        }

        // ── СТЕЙТ-МАШИНА ─────────────────────────────
        var state = _userStates[chatId];
        var draft = _drafts[chatId];

        switch (state)
        {
            // ── IDLE ───────────────────────────────────
            case AdminState.Idle:
                await botClient.SendMessage(chatId, "Введіть /add, щоб додати питання.", cancellationToken: ct);
                break;

            case AdminState.WaitingForCategories:
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    var category = dbContext.Categories.FirstOrDefault(c => c.Name == text);

                    if (category == null)
                    {
                        await botClient.SendMessage(chatId, "⚠️ Будь ласка, оберіть категорію за допомогою кнопок.",
                             cancellationToken: ct);
                        return;
                    }

                    draft.CategoryId = category.Id;
                }

                _userStates[chatId] = AdminState.WaitingForType;

                var typeKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "0. Звичайне (1 відповідь)", "1. Декілька правильних" },
                    new KeyboardButton[] { "2. Правда / Неправда", "3. Відповідність (пари)" },
                    new KeyboardButton[] { "4. Послідовність дій", "7. Введення числа" },
                    new KeyboardButton[] { "8. Коротка відповідь" }
                }) { ResizeKeyboard = true, OneTimeKeyboard = true };
                
                await botClient.SendMessage(chatId, $"✅ Вибрано категорію: *{{text}}*\\n\\nТепер оберіть *тип питання*:",
                    parseMode: ParseMode.Markdown, replyMarkup: typeKeyboard, cancellationToken: ct);
                break;
            
                
                
            // ── ВИБІР ТИПУ ────────────────────────────
            case AdminState.WaitingForType:
                var typePart = text.Split('.')[0].Trim();
                if (int.TryParse(typePart, out int typeInt) && Enum.IsDefined(typeof(QuestionType), typeInt))
                {
                    draft.Type = (QuestionType)typeInt;
                    _userStates[chatId] = AdminState.WaitingForQuestionText;
                    await botClient.SendMessage(chatId, $"✅ Тип: *{GetTypeName(draft.Type)}*\n\nВведіть текст питання:",
                        parseMode: ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: ct);
                }
                else
                    await botClient.SendMessage(chatId, "⚠️ Оберіть тип кнопкою або введіть цифру (0-8).", cancellationToken: ct);
                break;

            // ── ТЕКСТ ПИТАННЯ ─────────────────────────
            case AdminState.WaitingForQuestionText:
                if (string.IsNullOrWhiteSpace(text)) return;
                draft.Text = text;
                _userStates[chatId] = AdminState.WaitingForPhoto;
                await botClient.SendMessage(chatId,
                    "✅ Текст збережено.\n\n📷 Якщо потрібна картинка — надішліть фото або файл.\nЯкщо ні — напишіть *'-'*",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            // ── ФОТО ──────────────────────────────────
            case AdminState.WaitingForPhoto:
                if (message.Type == MessageType.Photo || message.Type == MessageType.Document)
                {
                    var fileId = message.Type == MessageType.Photo
                        ? message.Photo!.Last().FileId
                        : message.Document!.FileId;
                    var fileInfo = await botClient.GetFile(fileId, ct);
                    var ext = Path.GetExtension(fileInfo.FilePath);
                    if (string.IsNullOrEmpty(ext)) ext = ".jpg";

                    using var ms = new MemoryStream();
                    await botClient.DownloadFile(fileInfo.FilePath!, ms, ct);
                    ms.Position = 0;
                    draft.ImageUrl = await _imageStorage.SaveImageAsync(ms, ext, ct);
                    await botClient.SendMessage(chatId, "🖼 Картинку збережено!", cancellationToken: ct);
                }
                else if (text != "-")
                {
                    await botClient.SendMessage(chatId, "⚠️ Надішліть фото/файл або напишіть '-'.", cancellationToken: ct);
                    return;
                }

                // Після фото — переходимо до потрібного стану залежно від типу
                await GoToAnswerStateAsync(botClient, chatId, draft, ct);
                break;

            // ── ВАРІАНТИ ВІДПОВІДЕЙ (Single / Multiple) ─
            case AdminState.WaitingForAnswers:
                if (string.IsNullOrWhiteSpace(text)) return;
                var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 2)
                {
                    await botClient.SendMessage(chatId, "⚠️ Потрібно мінімум 2 варіанти!", cancellationToken: ct);
                    return;
                }
                draft.Answers = lines.ToList();
                _userStates[chatId] = AdminState.WaitingForCorrectAnswer;

                var isMulti = draft.Type == QuestionType.MultipleChoice;
                var hint = isMulti
                    ? "Введіть НОМЕРИ правильних варіантів через кому або пробіл (напр.: *1, 3*):\n\n"
                    : "Введіть НОМЕР правильного варіанту:\n\n";
                var list = string.Join("\n", lines.Select((l, i) => $"{i + 1}. {l}"));
                await botClient.SendMessage(chatId, hint + list, parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            // ── ПРАВИЛЬНА ВІДПОВІДЬ (число/а) ──────────
            case AdminState.WaitingForCorrectAnswer:
                var parts = text.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var indices = new List<int>();
                bool valid = true;
                foreach (var p in parts)
                {
                    if (int.TryParse(p.Trim(), out int idx) && idx >= 1 && idx <= draft.Answers.Count)
                        indices.Add(idx);
                    else { valid = false; break; }
                }
                if (!valid || indices.Count == 0)
                {
                    await botClient.SendMessage(chatId,
                        $"⚠️ Введіть цифри від 1 до {draft.Answers.Count}. Наприклад: 1 або 1, 3", cancellationToken: ct);
                    return;
                }
                if (draft.Type != QuestionType.MultipleChoice && indices.Count > 1)
                {
                    await botClient.SendMessage(chatId, "⚠️ Для цього типу потрібна лише ОДНА відповідь.", cancellationToken: ct);
                    return;
                }
                draft.CorrectAnswerIndices = indices;
                _userStates[chatId] = AdminState.WaitingForExplanation;
                await AskForExplanation(botClient, chatId, ct);
                break;

            // ── ПРАВДА / НЕПРАВДА ─────────────────────
            case AdminState.WaitingForTrueFalseAnswer:
                if (text != "Так" && text != "Ні")
                {
                    var tfKeyboard = new ReplyKeyboardMarkup(new[]
                        { new KeyboardButton[] { "Так", "Ні" } })
                        { ResizeKeyboard = true, OneTimeKeyboard = true };
                    await botClient.SendMessage(chatId, "⚠️ Оберіть кнопку: *Так* або *Ні*",
                        parseMode: ParseMode.Markdown, replyMarkup: tfKeyboard, cancellationToken: ct);
                    return;
                }
                // "Так" = індекс 1, "Ні" = індекс 2
                draft.Answers = new List<string> { "Так", "Ні" };
                draft.CorrectAnswerIndices = new List<int> { text == "Так" ? 1 : 2 };
                _userStates[chatId] = AdminState.WaitingForExplanation;
                await botClient.SendMessage(chatId, $"✅ Правильна відповідь: *{text}*",
                    parseMode: ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: ct);
                await AskForExplanation(botClient, chatId, ct);
                break;

            // ── ВІДПОВІДНІСТЬ (ПАРИ) ──────────────────
            case AdminState.WaitingForMatchingPairs:
                if (string.IsNullOrWhiteSpace(text)) return;
                var pairLines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (pairLines.Length < 2 || !pairLines.All(l => l.Contains('=')))
                {
                    await botClient.SendMessage(chatId,
                        "⚠️ Введіть пари у форматі *Ліве = Праве*, кожна з нового рядка.\n" +
                        "Приклад:\nЗнак 1 = Головна дорога\nЗнак 2 = Поступитися\n\nМінімум 2 пари.",
                        parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return;
                }
                // Зберігаємо як "Ліве|||Праве"
                draft.MatchingPairs = pairLines.Select(l =>
                {
                    var splitIdx = l.IndexOf('=');
                    return l[..splitIdx].Trim() + "|||" + l[(splitIdx + 1)..].Trim();
                }).ToList();
                _userStates[chatId] = AdminState.WaitingForExplanation;
                await botClient.SendMessage(chatId, $"✅ Збережено {draft.MatchingPairs.Count} пар.", cancellationToken: ct);
                await AskForExplanation(botClient, chatId, ct);
                break;

            // ── ПОСЛІДОВНІСТЬ ─────────────────────────
            case AdminState.WaitingForSequenceItems:
                if (string.IsNullOrWhiteSpace(text)) return;
                var seqLines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (seqLines.Length < 2)
                {
                    await botClient.SendMessage(chatId, "⚠️ Потрібно мінімум 2 елементи послідовності!", cancellationToken: ct);
                    return;
                }
                draft.SequenceItems = seqLines.ToList();
                _userStates[chatId] = AdminState.WaitingForExplanation;
                await botClient.SendMessage(chatId, $"✅ Послідовність із {seqLines.Length} кроків збережено.", cancellationToken: ct);
                await AskForExplanation(botClient, chatId, ct);
                break;

            // ── ВВЕДЕННЯ ЧИСЛА ────────────────────────
            case AdminState.WaitingForCorrectNumber:
                if (!decimal.TryParse(text.Trim().Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    await botClient.SendMessage(chatId, "⚠️ Введіть правильне число (наприклад: 60 або 3.5)", cancellationToken: ct);
                    return;
                }
                draft.CorrectTextAnswer = text.Trim();
                _userStates[chatId] = AdminState.WaitingForExplanation;
                await botClient.SendMessage(chatId, $"✅ Правильна цифра: *{text.Trim()}*", parseMode: ParseMode.Markdown, cancellationToken: ct);
                await AskForExplanation(botClient, chatId, ct);
                break;

            // ── КОРОТКА ВІДПОВІДЬ (текст) ─────────────
            case AdminState.WaitingForCorrectText:
                if (string.IsNullOrWhiteSpace(text)) return;
                draft.CorrectTextAnswer = text.Trim();
                _userStates[chatId] = AdminState.WaitingForExplanation;
                await botClient.SendMessage(chatId, $"✅ Правильна відповідь: *{text.Trim()}*", parseMode: ParseMode.Markdown, cancellationToken: ct);
                await AskForExplanation(botClient, chatId, ct);
                break;

            // ── ПОЯСНЕННЯ + ЗБЕРЕЖЕННЯ ────────────────
            case AdminState.WaitingForExplanation:
                if (string.IsNullOrWhiteSpace(text)) return;
                draft.Explanation = text == "-" ? null : text;
                await SaveQuestionAsync(chatId, draft, ct);
                _userStates[chatId] = AdminState.Idle;
                await botClient.SendMessage(chatId,
                    "🎉 *ПИТАННЯ ЗБЕРЕЖЕНО!* 🎉\n\nВведіть /add щоб додати ще одне.",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;
        }
    }

    // ── ДОПОМІЖНІ МЕТОДИ ─────────────────────────────

    // Після фото визначаємо наступний крок залежно від типу питання
    private async Task GoToAnswerStateAsync(ITelegramBotClient botClient, long chatId, QuestionDraft draft, CancellationToken ct)
    {
        switch (draft.Type)
        {
            case QuestionType.SingleChoice:
            case QuestionType.MultipleChoice:
                _userStates[chatId] = AdminState.WaitingForAnswers;
                await botClient.SendMessage(chatId,
                    "✅ Переходимо до відповідей.\nВведіть варіанти — *кожен з нового рядка* в одному повідомленні:",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            case QuestionType.TrueFalse:
                _userStates[chatId] = AdminState.WaitingForTrueFalseAnswer;
                var tfKeyboard = new ReplyKeyboardMarkup(new[]
                    { new KeyboardButton[] { "Так", "Ні" } })
                    { ResizeKeyboard = true, OneTimeKeyboard = true };
                await botClient.SendMessage(chatId,
                    "✅ Тип *Правда/Неправда*. Яка правильна відповідь?",
                    parseMode: ParseMode.Markdown, replyMarkup: tfKeyboard, cancellationToken: ct);
                break;

            case QuestionType.Matching:
                _userStates[chatId] = AdminState.WaitingForMatchingPairs;
                await botClient.SendMessage(chatId,
                    "✅ Введіть пари у форматі *Ліве = Праве*, кожна з нового рядка.\n\n" +
                    "Приклад:\n`Знак 1 = Головна дорога`\n`Знак 2 = Поступитися`",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            case QuestionType.Sequence:
                _userStates[chatId] = AdminState.WaitingForSequenceItems;
                await botClient.SendMessage(chatId,
                    "✅ Введіть елементи послідовності у *правильному порядку*, кожен з нового рядка.\n\n" +
                    "Приклад:\n`1. Увімкнути покажчик`\n`2. Перевірити дзеркало`\n`3. Виконати маневр`",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            case QuestionType.NumberInput:
                _userStates[chatId] = AdminState.WaitingForCorrectNumber;
                await botClient.SendMessage(chatId,
                    "✅ Введіть *правильне число* (наприклад: 60 або 3.5):",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            case QuestionType.ShortAnswer:
                _userStates[chatId] = AdminState.WaitingForCorrectText;
                await botClient.SendMessage(chatId,
                    "✅ Введіть *правильну коротку відповідь* (текст):",
                    parseMode: ParseMode.Markdown, cancellationToken: ct);
                break;

            default:
                // ImageAreaClick, DragAndDrop — складні типи, не підтримуються в боті повністю
                _userStates[chatId] = AdminState.WaitingForExplanation;
                await botClient.SendMessage(chatId,
                    "⚠️ Цей тип питання не підтримує введення відповідей через бота. " +
                    "Питання буде збережено без відповідей (для ручного налаштування).",
                    cancellationToken: ct);
                await AskForExplanation(botClient, chatId, ct);
                break;
        }
    }

    private async Task AskForExplanation(ITelegramBotClient botClient, long chatId, CancellationToken ct)
    {
        await botClient.SendMessage(chatId,
            "📖 Останній крок: введіть *пояснення* до цього питання.\nАбо напишіть *'-'* якщо пояснення не потрібне.",
            parseMode: ParseMode.Markdown, cancellationToken: ct);
    }

    // Збереження питання в БД залежно від типу
    private async Task SaveQuestionAsync(long chatId, QuestionDraft draft, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        List<Answer> answers;
       
        

        switch (draft.Type)
        {
            case QuestionType.SingleChoice:
            case QuestionType.MultipleChoice:
            case QuestionType.TrueFalse:
                // Прості варіанти відповідей
                answers = draft.Answers.Select((text, i) => new Answer
                {
                    Text = text,
                    IsCorrect = draft.CorrectAnswerIndices.Contains(i + 1)
                }).ToList();
                break;

            case QuestionType.Matching:
                // Кожна пара — це правильна відповідь. Зберігаємо "Ліве|||Праве"
                answers = draft.MatchingPairs.Select(pair => new Answer
                {
                    Text = pair,   // формат: "Знак 1|||Головна дорога"
                    IsCorrect = true
                }).ToList();
                break;

            case QuestionType.Sequence:
                // Зберігаємо у правильному порядку. Фронтенд перемішує.
                answers = draft.SequenceItems.Select((item, i) => new Answer
                {
                    Text = item,
                    IsCorrect = true,  // всі є частиною правильної послідовності
                    SortOrder = i + 1  // 1-based correct order
                }).ToList();
                break;

            case QuestionType.NumberInput:
            case QuestionType.ShortAnswer:
                // Одна правильна відповідь — введений текст/число
                answers = new List<Answer>
                {
                    new Answer { Text = draft.CorrectTextAnswer ?? "", IsCorrect = true }
                };
                break;

            default:
                answers = new List<Answer>();
                break;
        }

        var question = new Question
        {
            Type = draft.Type,
            Text = draft.Text,
            ImageUrl = draft.ImageUrl,
            Explanation = draft.Explanation,
            CategoryId = draft.CategoryId,
            Difficulty = DifficultyLevel.Medium,
            Answers = answers
        };

        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync(ct);
    }

    // Назви типів для відображення у боті
    private static string GetTypeName(QuestionType type) => type switch
    {
        QuestionType.SingleChoice   => "Одна правильна відповідь",
        QuestionType.MultipleChoice => "Декілька правильних відповідей",
        QuestionType.TrueFalse      => "Правда / Неправда",
        QuestionType.Matching       => "Встановлення відповідності (пари)",
        QuestionType.Sequence       => "Послідовність дій",
        QuestionType.ImageAreaClick => "Клік по картинці",
        QuestionType.DragAndDrop    => "Перетягування (Drag & Drop)",
        QuestionType.NumberInput    => "Введення числа",
        QuestionType.ShortAnswer    => "Коротка відповідь",
        _                           => "Невідомий тип"
    };

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        _logger.LogError(exception.ToString());
        return Task.CompletedTask;
    }
}
