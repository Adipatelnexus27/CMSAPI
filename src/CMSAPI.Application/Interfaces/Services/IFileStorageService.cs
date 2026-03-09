namespace CMSAPI.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
}

