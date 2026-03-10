using CMS.Application.Interfaces.Services;

namespace CMS.Infrastructure.Documents;

public sealed class FileSystemDocumentStorageService : IDocumentStorageService
{
    private readonly string _storageRoot;

    public FileSystemDocumentStorageService()
    {
        _storageRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "uploads", "claims"));
    }

    public async Task<string> SaveAsync(string claimNumber, string originalFileName, Stream contentStream, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_storageRoot);

        var safeClaimSegment = SanitizePathSegment(claimNumber);
        var claimDirectory = Path.Combine(_storageRoot, safeClaimSegment);
        Directory.CreateDirectory(claimDirectory);

        var safeFileName = SanitizeFileName(originalFileName);
        var extension = Path.GetExtension(safeFileName);
        var generatedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(claimDirectory, generatedName);

        await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await contentStream.CopyToAsync(fileStream, cancellationToken);

        return fullPath;
    }

    public Task<Stream> OpenReadAsync(string storedFilePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(storedFilePath))
        {
            throw new InvalidOperationException("Stored file path is missing.");
        }

        var normalizedPath = Path.GetFullPath(storedFilePath);
        if (!normalizedPath.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Document path is outside of the configured storage root.");
        }

        if (!File.Exists(normalizedPath))
        {
            throw new InvalidOperationException("Document file was not found in storage.");
        }

        Stream stream = new FileStream(
            normalizedPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult(stream);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "document.bin" : sanitized;
    }

    private static string SanitizePathSegment(string input)
    {
        var invalidChars = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct().ToArray();
        var sanitized = new string(input.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "unknown-claim" : sanitized;
    }
}
