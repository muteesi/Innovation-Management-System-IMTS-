namespace IMTS.Models;

public class UserModel
{
    public string Username { get; init; } = "";
    public string FullName { get; init; } = "";
    public string Role { get; init; } = "";
    public string Department { get; init; } = "";
    public string AvatarUrl { get; init; } = "";
}

public class IdeaModel
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Summary { get; init; } = "";
    public string Category { get; init; } = "";
    public string Status { get; init; } = "";
    public string Submitter { get; init; } = "";
    public string Department { get; init; } = "";
    public DateTime SubmittedDate { get; init; }
    public string Priority { get; init; } = "";
}

public class NotificationModel
{
    public int Id { get; init; }
    public string Message { get; init; } = "";
    public string Type { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public bool IsRead { get; init; }
}

public class AuditLogEntry
{
    public int Id { get; init; }
    public string Event { get; init; } = "";
    public string User { get; init; } = "";
    public string IpAddress { get; init; } = "";
    public DateTime Timestamp { get; init; }
    public string Severity { get; init; } = "";
}
