using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;
using CMS.Application.Models;

namespace CMS.Application.Services;

public sealed class DocumentService : IDocumentService
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "General",
        "Evidence",
        "AccidentPhoto",
        "PoliceReport",
        "MedicalReport",
        "Invoice",
        "Settlement"
    };

    private readonly IDocumentRepository _documentRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IDocumentStorageService _documentStorageService;

    public DocumentService(IDocumentRepository documentRepository, IClaimRepository claimRepository, IDocumentStorageService documentStorageService)
    {
        _documentRepository = documentRepository;
        _claimRepository = claimRepository;
        _documentStorageService = documentStorageService;
    }

    public async Task<UploadDocumentResponseDto> UploadDocumentAsync(
        Guid claimId,
        string? documentCategory,
        Guid? documentGroupId,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        Stream contentStream,
        Guid? uploadedByUserId,
        CancellationToken cancellationToken)
    {
        ValidateDocumentUpload(originalFileName, fileSizeBytes);
        var normalizedCategory = NormalizeCategory(documentCategory);

        var claim = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        var storedFilePath = await _documentStorageService.SaveAsync(claim.ClaimNumber, originalFileName, contentStream, cancellationToken);

        var document = await _documentRepository.AddDocumentVersionAsync(
            claimId,
            originalFileName,
            storedFilePath,
            contentType,
            fileSizeBytes,
            normalizedCategory,
            documentGroupId,
            uploadedByUserId,
            cancellationToken);

        return new UploadDocumentResponseDto
        {
            ClaimDocumentId = document.ClaimDocumentId,
            ClaimId = document.ClaimId,
            OriginalFileName = document.OriginalFileName,
            DocumentCategory = document.DocumentCategory,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAtUtc = document.UploadedAtUtc,
            DocumentGroupId = document.DocumentGroupId,
            VersionNumber = document.VersionNumber,
            IsLatest = document.IsLatest,
            UploadedByUserId = document.UploadedByUserId
        };
    }

    public async Task<IReadOnlyList<ClaimDocumentDto>> GetClaimDocumentsAsync(Guid claimId, bool latestOnly, CancellationToken cancellationToken)
    {
        _ = await _claimRepository.GetClaimByIdAsync(claimId, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        var documents = await _documentRepository.GetClaimDocumentsAsync(claimId, latestOnly, cancellationToken);
        return documents.Select(MapForResponse).ToList();
    }

    public async Task<ClaimDocumentDto> GetDocumentAsync(Guid claimDocumentId, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetDocumentByIdAsync(claimDocumentId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        return MapForResponse(document);
    }

    public async Task<IReadOnlyList<ClaimDocumentDto>> GetDocumentVersionsAsync(Guid claimDocumentId, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetDocumentByIdAsync(claimDocumentId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        var versions = await _documentRepository.GetDocumentVersionsAsync(document.DocumentGroupId, cancellationToken);
        return versions.Select(MapForResponse).ToList();
    }

    public async Task<DocumentPreviewResult> GetDocumentPreviewAsync(Guid claimDocumentId, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetDocumentByIdAsync(claimDocumentId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        var stream = await _documentStorageService.OpenReadAsync(document.StoredFilePath, cancellationToken);

        return new DocumentPreviewResult
        {
            ContentStream = stream,
            ContentType = string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
            FileName = document.OriginalFileName
        };
    }

    private static string NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "General";
        }

        var normalized = category.Trim();
        if (!AllowedCategories.Contains(normalized))
        {
            throw new InvalidOperationException("Invalid document category.");
        }

        return normalized;
    }

    private static void ValidateDocumentUpload(string originalFileName, long fileSizeBytes)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new InvalidOperationException("Document name is required.");
        }

        if (fileSizeBytes <= 0)
        {
            throw new InvalidOperationException("Document is empty.");
        }

        if (fileSizeBytes > 25 * 1024 * 1024)
        {
            throw new InvalidOperationException("Document exceeds 25 MB limit.");
        }
    }

    private static ClaimDocumentDto MapForResponse(DocumentRecordDto document)
    {
        return new ClaimDocumentDto
        {
            ClaimDocumentId = document.ClaimDocumentId,
            ClaimId = document.ClaimId,
            OriginalFileName = document.OriginalFileName,
            DocumentCategory = document.DocumentCategory,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAtUtc = document.UploadedAtUtc,
            DocumentGroupId = document.DocumentGroupId,
            VersionNumber = document.VersionNumber,
            IsLatest = document.IsLatest,
            UploadedByUserId = document.UploadedByUserId
        };
    }
}
