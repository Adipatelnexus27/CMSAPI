using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CMSAPI.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email queued. From: {FromAddress}, To: {To}, Subject: {Subject}, BodyLength: {BodyLength}",
            _options.FromAddress,
            to,
            subject,
            body.Length);

        return Task.CompletedTask;
    }
}

