using IMTS.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMTS.Pages.ItAdmin;

public class DashboardModel : BasePageModel
{
    public UserModel CurrentUser => StubData.CurrentAdmin;
    public int TotalUsers => 254;
    public int ActiveUsers => 238;
    public int LockedAccounts => 4;
    public int FailedLogins => 18;

    public List<AuditLogEntry> AuditLogs => StubData.GetAuditLogs();
    public List<DashboardUser> Users { get; set; } = new();

    public void OnGet()
    {
        SetCommonViewData("Admin Dashboard", "Innovation Management System", "System administration and user management", CurrentUser);

        Users = new()
        {
            new() { Username = "m.okello", FullName = "Michael Okello", Role = "Staff", Department = "Operations", Status = "Active" },
            new() { Username = "s.namono", FullName = "Sarah Namono", Role = "Staff", Department = "Innovation Team", Status = "Active" },
            new() { Username = "p.mukasa", FullName = "Peter Mukasa", Role = "Reviewer", Department = "IT Security", Status = "Locked" },
            new() { Username = "g.nakato", FullName = "Grace Nakato", Role = "Staff", Department = "Human Resources", Status = "Disabled" }
        };
    }
}

public class DashboardUser
{
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public string Department { get; set; } = "";
    public string Status { get; set; } = "";
}
