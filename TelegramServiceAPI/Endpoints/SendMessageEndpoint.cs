using FastEndpoints;
using TelegramServiceAPI.DTO.Requests;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Endpoints;

public class SendMessageEndpoint(ITelegramMessageReSenderService reSenderService, ILogger<SendMessageEndpoint> logger) : Endpoint<SendMessageRequest>
{
    private readonly ITelegramMessageReSenderService _reSenderService = reSenderService;
    private readonly ILogger<SendMessageEndpoint> _logger = logger;

    public override void Configure()
    {
        Post("/api/telegram/send-message");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Принимает текст и отправляет его в Telegram";
            s.Description = "Если указан порт, сообщение уйдет сотрудникам, подписанным на этот порт через бота.";
        });
    }

    public override async Task HandleAsync(SendMessageRequest req, CancellationToken ct)
    {
        _logger.LogInformation("Получен запрос на отправку сообщения. Порт: {Port}", req.Port.ToString() ?? "не указан");

        if (string.IsNullOrWhiteSpace(req.Description))
            ThrowError("Текст сообщения не может быть пустым.");

        await _reSenderService.SendTextMessageAsync(
            req.Description,
            req.Port,
            ct
        );

        _logger.LogInformation("Сообщение \n" + $"{req.Description}\n" + $"успешно обработано и отправлено на владельца порта {req.Port}XXX.");

        await Send.OkAsync(new { Success = true, Message = "Сообщение успешно отправлено в Telegram." }, ct);
    }
}