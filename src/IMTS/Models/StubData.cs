namespace IMTS.Models;

public static class StubData
{
    public static readonly UserModel CurrentStaff = new()
    {
        Username = "j.doe",
        FullName = "Jonathan Doe",
        Role = "Staff",
        Department = "Information Technology",
        AvatarUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBk3WQTswPIr0Jl5tuV4MiL0FNAjrjdZygecSzuqU9k68yUxMD8ypOkB0Utq1c6DFrIbFp8-Hno8OtDkUeVR7QGVx1f45JGbS1rTRBChgWZC5_KUNEPzDyoz_9dPM_567uqWHODI_nIH7fkj5j-5AAt5edKvviVCsBYJz0XMhZ0vojkrGeq0B7mYUOh-XyJpk450rpZjpaOnDxgwvbqS6JeyJqxZwwjttr-vOW9P9JeqJySGAqNRNYUbw"
    };

    public static readonly UserModel CurrentReviewer = new()
    {
        Username = "r.namaganda",
        FullName = "Rose Namaganda",
        Role = "Reviewer",
        Department = "Innovation Team",
        AvatarUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAPEi3Dq35RpA-uhErae5k73Rn07gQW5Ch0R1GQRP5wkqXDhsxb9GAlQuC9tMrjUPJI6v265EVirfwVLnBLt0mEpSJ6qnvUBSKL0roA2F71WsHQ01Reqn7-VVlXzaNopK_5VJQnwxxlPO3XI9l2p5ACWkgSgVqUbBVOMSVqPQJSFpU_TsLrE9YL50ZLnn13q7qmL1cHQ0G-JTOBBl1gVPhf2_BeXlTwEUyatZBo93meEwFELH8JLRBTRQ"
    };

    public static readonly UserModel CurrentAdmin = new()
    {
        Username = "j.katumba",
        FullName = "Joseph Katumba",
        Role = "ITAdmin",
        Department = "IT Security",
        AvatarUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAOA7AdNEpWLgbGGy3Jcg9rfCbuPOsRk6eiAXKT2w3Cmi6ui4kQ8CzVFET1vhqFtEO0HgumiITGkkkRdXnhfzPMIbLsK7SJYh9C-N_0FRm0MoLvJf9vSCAkvWbMcX1klqcIL_L57DPKsUZF3EQLcAU2XFzic1RteNI1-LjBrS_JysfS7npG5v5MI11_UGrjF2ppW3BiQq_9TNFwmtOrH-ZBxxdDfwL7jFEPyFGFZmXN0GEKPgMOW58Q5w"
    };

    public static List<IdeaModel> GetIdeas() => new()
    {
        new() { Id = 1, Title = "Digital KYC Blockchain Ledger", Summary = "A decentralized system for verifying customer identity across all commercial banks.", Category = "Fintech Solutions", Status = "Under Review", Submitter = "Jonathan Doe", Department = "IT Dept", SubmittedDate = DateTime.Parse("2024-10-12") },
        new() { Id = 2, Title = "Micro-Loan AI Risk Engine", Summary = "Using ML algorithms to assess creditworthiness for unbanked populations.", Category = "Risk Management", Status = "In Development", Submitter = "Jonathan Doe", Department = "IT Dept", SubmittedDate = DateTime.Parse("2024-09-28") },
        new() { Id = 3, Title = "Cashless Marketplace Initiative", Summary = "Strategy for promoting digital payment systems in traditional local markets.", Category = "Public Policy", Status = "Approved", Submitter = "Jonathan Doe", Department = "IT Dept", SubmittedDate = DateTime.Parse("2024-08-15") },
        new() { Id = 4, Title = "Fraud Detection Dashboard", Summary = "Real-time dashboard integrating transaction data from multiple banks.", Category = "Security", Status = "Under Review", Submitter = "Sarah Namono", Department = "Operations", SubmittedDate = DateTime.Parse("2024-07-02") },
        new() { Id = 5, Title = "Blockchain Remittance Portal", Summary = "Cross-border remittance solution using blockchain technology.", Category = "FinTech", Status = "Pending", Submitter = "O. Mubiru", Department = "IT Dept", SubmittedDate = DateTime.Parse("2023-10-12") },
        new() { Id = 6, Title = "AI-Driven Fraud Detection", Summary = "Advanced anomaly detection for transaction monitoring.", Category = "Security", Status = "In Review", Submitter = "J. Namubiru", Department = "Risk Management", SubmittedDate = DateTime.Parse("2023-10-14") },
        new() { Id = 7, Title = "Digital Asset Custody Framework", Summary = "Secure custody framework for digital assets.", Category = "Policy", Status = "Pending", Submitter = "K. Okello", Department = "Legal & Compliance", SubmittedDate = DateTime.Parse("2023-10-15") },
        new() { Id = 8, Title = "Central Bank Digital Currency (CBDC) Pilot", Summary = "Pilot program for a digital shilling.", Category = "Innovation", Status = "Approved", Submitter = "S. Kateregga", Department = "Monetary Policy", SubmittedDate = DateTime.Parse("2023-10-16") },
        new() { Id = 9, Title = "Unified HR Portal Revamp", Summary = "Modernizing the HR portal for improved employee experience.", Category = "Operations", Status = "Declined", Submitter = "L. Atwine", Department = "Human Resources", SubmittedDate = DateTime.Parse("2023-10-17") }
    };

    public static List<NotificationModel> GetNotifications() => new()
    {
        new() { Id = 1, Message = "Your idea 'Digital KYC Blockchain Ledger' has been moved to 'Under Review'", Type = "status", CreatedAt = DateTime.Now.AddHours(-2), IsRead = false },
        new() { Id = 2, Message = "New comment from Rose Namaganda on 'Micro-Loan AI Risk Engine'", Type = "comment", CreatedAt = DateTime.Now.AddHours(-5), IsRead = false },
        new() { Id = 3, Message = "Reminder: Q4 Idea submission deadline is October 25th", Type = "reminder", CreatedAt = DateTime.Now.AddDays(-1), IsRead = false }
    };

    public static List<AuditLogEntry> GetAuditLogs() => new()
    {
        new() { Id = 1, Event = "Successful Login", User = "j.katumba", IpAddress = "192.168.1.45", Timestamp = DateTime.Today.AddHours(9).AddMinutes(24), Severity = "info" },
        new() { Id = 2, Event = "Account Locked", User = "m.okello", IpAddress = "System Rule Auto", Timestamp = DateTime.Today.AddHours(8).AddMinutes(12), Severity = "warning" },
        new() { Id = 3, Event = "Policy Update", User = "s.admin", IpAddress = "Password Complexity", Timestamp = DateTime.Today.AddDays(-1).AddHours(16).AddMinutes(55), Severity = "info" },
        new() { Id = 4, Event = "Failed Login", User = "unknown", IpAddress = "41.210.144.22", Timestamp = DateTime.Today.AddDays(-1).AddHours(23).AddMinutes(30), Severity = "error" }
    };

    public static Dictionary<string, string> GetRoleRoutes() => new()
    {
        ["Staff"] = "/StaffUser/Dashboard",
        ["Reviewer"] = "/InnovationTeam/Dashboard",
        ["ITAdmin"] = "/ItAdmin/Dashboard"
    };
}
