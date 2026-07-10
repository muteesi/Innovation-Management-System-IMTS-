using IMTS.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMTS.Pages.ItAdmin;

public class ReportsModel : BasePageModel
{
    public UserModel CurrentUser => StubData.CurrentAdmin;

    [BindProperty]
    public string ReportType { get; set; } = "";

    [BindProperty]
    public DateTime? DateFrom { get; set; }

    [BindProperty]
    public DateTime? DateTo { get; set; }

    [BindProperty]
    public string ReportFormat { get; set; } = "PDF";

    public string SuccessMessage { get; set; } = "";

    public List<GeneratedReport> Reports { get; set; } = new();

    public void OnGet()
    {
        SetCommonViewData("Reports", "System Reports", "Generate and view system reports", CurrentUser);
        LoadReports();
    }

    public IActionResult OnPostGenerate()
    {
        SetCommonViewData("Reports", "System Reports", "Generate and view system reports", CurrentUser);
        LoadReports();

        if (string.IsNullOrWhiteSpace(ReportType))
        {
            ModelState.AddModelError("", "Please select a report type.");
            return Page();
        }

        SuccessMessage = $"Report '{ReportType}' generated successfully in {ReportFormat} format.";
        Reports.Insert(0, new()
        {
            Id = Reports.Count + 1,
            Name = ReportType,
            Format = ReportFormat,
            GeneratedAt = DateTime.Now,
            GeneratedBy = CurrentUser.FullName,
            Status = "Ready"
        });

        ReportType = "";
        DateFrom = null;
        DateTo = null;
        ReportFormat = "PDF";
        return Page();
    }

    private void LoadReports()
    {
        Reports = new()
        {
            new() { Id = 1, Name = "User Activity Report", Format = "PDF", GeneratedAt = DateTime.Today.AddDays(-1), GeneratedBy = CurrentUser.FullName, Status = "Ready" },
            new() { Id = 2, Name = "System Health Report", Format = "Excel", GeneratedAt = DateTime.Today.AddDays(-3), GeneratedBy = CurrentUser.FullName, Status = "Ready" },
            new() { Id = 3, Name = "Security Audit Report", Format = "CSV", GeneratedAt = DateTime.Today.AddDays(-7), GeneratedBy = CurrentUser.FullName, Status = "Ready" },
            new() { Id = 4, Name = "User Activity Report", Format = "PDF", GeneratedAt = DateTime.Today.AddDays(-14), GeneratedBy = "System Auto-Generate", Status = "Ready" }
        };
    }

    public List<string> ReportTypes => new()
    {
        "User Activity Report",
        "System Health Report",
        "Security Audit Report",
        "Failed Login Report",
        "Account Lockout Report",
        "Role Change Report"
    };
}

public class GeneratedReport
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Format { get; set; } = "";
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = "";
    public string Status { get; set; } = "";
}
