namespace IMTS.Shared.Dtos;

public record SendEmailRequest(string ToAddress, string Subject, string BodyHtml, string? TemplateKey);

public record EmailQueueItemDto(int Id, string ToAddress, string Subject, string Status, int RetryCount, DateTimeOffset? SentAt, string? ErrorMessage);
