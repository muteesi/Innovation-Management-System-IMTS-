namespace IMTS.Shared.Dtos;

public record NotificationListItemDto(
    int Id,
    string Title,
    string Message,
    string Type,
    string Date,
    bool Read,
    string? ActionUrl);

public record NotificationCreateRequest(string Title, string Message, string Type, string RecipientUserId, string? ActionUrl);

public record NotificationSummaryDto(int TotalCount, int UnreadCount);
