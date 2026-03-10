using CMS.Application.Interfaces.Services;

namespace CMS.Infrastructure.Documents;

public sealed class FileSystemDocumentStorageService : IDocumentStorageService
{
    private readonly string _storageRoot;

    public FileSystemDocumentStorageService()
    {
        _storageRoot = Path.Combine(AppContext.BaseDirectory, "uploads", "claims");
    }

    public async Task<string> SaveAsync(string claimNumber, string originalFileName, Stream contentStream, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_storageRoot);

        var safeFileName = SanitizeFileName(originalFileName);
        var extension = Path.GetExtension(safeFileName);
        var generatedName = $"{claimNumber}_{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_storageRoot, generatedName);

        await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await contentStream.CopyToAsync(fileStream, cancellationToken);

        return fullPath;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "document.bin" : sanitized;
    }
}
