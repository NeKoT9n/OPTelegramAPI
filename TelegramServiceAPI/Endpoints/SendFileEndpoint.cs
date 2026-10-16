using FastEndpoints;
using TelegramServiceAPI.DTO.Requests;
using TelegramServiceAPI.Model.Abstraction;

namespace TelegramServiceAPI.Endpoints;

public class SendFileEndpoint(ITelegramFileSenderService fileSenderService, ILogger<SendFileEndpoint> logger) : Endpoint<SendFileRequest>
{
    private readonly ITelegramFileSenderService _fileSenderService = fileSenderService;
    private readonly ILogger<SendFileEndpoint> _logger = logger;

    public override void Configure()
    {
        Post("/api/telegram/send");
        AllowAnonymous();
        AllowFileUploads();

        Summary(s =>
        {
            s.Summary = "Принимает файл и описание, отправляет их в Telegram-группу";
            s.Description = "Потоковая пересылка файла без сохранения на диск через Core-сервис.";
        });
    }

    public override async Task HandleAsync(SendFileRequest req, CancellationToken ct)
    {
        _logger.LogInformation("Получен запрос на отправку файла: {FileName}", req.File?.FileName);

        if (req.File == null || req.File.Length == 0)
            ThrowError("Файл не был передан или он пустой.");

        await using var stream = req.File.OpenReadStream();

        await _fileSenderService.SendFileAsync(
            stream,
            req.File.FileName,
            req.Description,
            ct
        );

        _logger.LogInformation("Файл {FileName} успешно отправлен в Telegram.", req.File.FileName);

        await Send.OkAsync(new { Success = true, Message = "Файл успешно отправлен в Telegram." }, ct);
    }
}
