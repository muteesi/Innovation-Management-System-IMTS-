namespace IMTS.Shared.Dtos;

public record ResourceListItemDto(
    int Id,
    string Title,
    string Description,
    string Category,
    string Type,
    string Size,
    string Icon,
    int Downloads,
    string Status,
    string? FileUrl,
    string? PreviewMetadata,
    DateTimeOffset? PublishedDate);

public record ResourceCreateRequest(string Title, string Description, string Category, string FileType, string FileName, string? FileUrl, string? StoragePath, string Status, string Version, string? PreviewMetadata);

public record ResourceUpdateRequest(string? Title, string? Description, string? Category, string? FileType, string? FileName, string? FileUrl, string? StoragePath, string? Status, string? Version, string? PreviewMetadata);

public record ResourceStatsDto(int Total, int Published, int Drafts, int Downloads);
