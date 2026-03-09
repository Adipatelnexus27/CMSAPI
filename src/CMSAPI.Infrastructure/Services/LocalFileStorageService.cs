using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CMSAPI.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _rootPath = options.Value.RootPath;
    }

    public async Task<string> SaveFileAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_rootPath);

        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var path = Path.Combine(_rootPath, safeName);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
        return path;
    }
}

