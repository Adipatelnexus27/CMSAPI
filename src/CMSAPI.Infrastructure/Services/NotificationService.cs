using CMSAPI.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CMSAPI.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(string userId, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notification queued for UserId: {UserId}. Message: {Message}", userId, message);
        return Task.CompletedTask;
    }
}

