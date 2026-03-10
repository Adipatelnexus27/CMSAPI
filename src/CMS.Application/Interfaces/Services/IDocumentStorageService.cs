namespace CMS.Application.Interfaces.Services;

public interface IDocumentStorageService
{
    Task<string> SaveAsync(string claimNumber, string originalFileName, Stream contentStream, CancellationToken cancellationToken);
}
