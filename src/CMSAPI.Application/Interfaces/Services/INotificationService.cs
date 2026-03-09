namespace CMSAPI.Application.Interfaces.Services;

public interface INotificationService
{
    Task NotifyAsync(string userId, string message, CancellationToken cancellationToken = default);
}

