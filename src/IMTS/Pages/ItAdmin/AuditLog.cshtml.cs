using IMTS.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMTS.Pages.ItAdmin;

public class AuditLogModel : BasePageModel
{
    public UserModel CurrentUser => StubData.CurrentAdmin;

    [BindProperty]
    public DateTime? DateFrom { get; set; }

    [BindProperty]
    public DateTime? DateTo { get; set; }

    [BindProperty]
    public string SeverityFilter { get; set; } = "";

    public List<AuditLogEntry> AllLogs => StubData.GetAuditLogs();
    public List<AuditLogEntry> FilteredLogs { get; set; } = new();

    public void OnGet()
    {
        SetCommonViewData("Audit Log", "Audit Trail", "System-wide audit log of all activities", CurrentUser);
        ApplyFilters();
    }

    public IActionResult OnPostFilter()
    {
        SetCommonViewData("Audit Log", "Audit Trail", "System-wide audit log of all activities", CurrentUser);
        ApplyFilters();
        return Page();
    }

    public IActionResult OnPostExport()
    {
        TempData["ExportMessage"] = "Export functionality will be available in the next release.";
        return RedirectToPage();
    }

    private void ApplyFilters()
    {
        var query = AllLogs.AsEnumerable();

        if (DateFrom.HasValue)
            query = query.Where(l => l.Timestamp >= DateFrom.Value);

        if (DateTo.HasValue)
            query = query.Where(l => l.Timestamp <= DateTo.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(SeverityFilter))
            query = query.Where(l => l.Severity.Equals(SeverityFilter, StringComparison.OrdinalIgnoreCase));

        FilteredLogs = query.OrderByDescending(l => l.Timestamp).ToList();
    }

    public static string GetSeverityBadgeClass(string severity) => severity.ToLower() switch
    {
        "error" => "bg-error/10 text-error border border-error/20",
        "warning" => "bg-warning/10 text-warning border border-warning/20",
        _ => "bg-info/10 text-info border border-info/20"
    };
}
