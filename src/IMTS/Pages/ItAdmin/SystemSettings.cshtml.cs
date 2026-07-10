using IMTS.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMTS.Pages.ItAdmin;

public class SystemSettingsModel : BasePageModel
{
    public UserModel CurrentUser => StubData.CurrentAdmin;

    [BindProperty]
    public string SystemName { get; set; } = "Innovation Management System";

    [BindProperty]
    public string SupportEmail { get; set; } = "support@bou.or.ug";

    [BindProperty]
    public bool MaintenanceMode { get; set; }

    [BindProperty]
    public int MinPasswordLength { get; set; } = 8;

    [BindProperty]
    public bool RequireSpecialChars { get; set; } = true;

    [BindProperty]
    public bool RequireNumbers { get; set; } = true;

    [BindProperty]
    public int SessionTimeoutMinutes { get; set; } = 30;

    [BindProperty]
    public bool Enforce2FA { get; set; }

    [BindProperty]
    public string SmtpHost { get; set; } = "smtp.bou.or.ug";

    [BindProperty]
    public int SmtpPort { get; set; } = 587;

    [BindProperty]
    public string SmtpUsername { get; set; } = "noreply@bou.or.ug";

    [BindProperty]
    public bool UseSsl { get; set; } = true;

    [BindProperty]
    public bool NotifyOnAccountLock { get; set; } = true;

    [BindProperty]
    public bool NotifyOnFailedLogin { get; set; } = true;

    [BindProperty]
    public bool NotifyOnNewUser { get; set; } = true;

    [BindProperty]
    public bool DailyDigestEnabled { get; set; }

    public string SuccessMessage { get; set; } = "";
    public string ActiveSection { get; set; } = "general";

    public void OnGet(string section)
    {
        SetCommonViewData("System Settings", "System Configuration", "Manage system-wide settings", CurrentUser);
        if (!string.IsNullOrEmpty(section))
            ActiveSection = section;
    }

    public IActionResult OnPostGeneral()
    {
        SetCommonViewData("System Settings", "System Configuration", "Manage system-wide settings", CurrentUser);
        ActiveSection = "general";
        SuccessMessage = "General settings saved successfully.";
        return Page();
    }

    public IActionResult OnPostSecurity()
    {
        SetCommonViewData("System Settings", "System Configuration", "Manage system-wide settings", CurrentUser);
        ActiveSection = "security";
        SuccessMessage = "Security settings saved successfully.";
        return Page();
    }

    public IActionResult OnPostEmail()
    {
        SetCommonViewData("System Settings", "System Configuration", "Manage system-wide settings", CurrentUser);
        ActiveSection = "email";
        SuccessMessage = "Email settings saved successfully.";
        return Page();
    }

    public IActionResult OnPostNotification()
    {
        SetCommonViewData("System Settings", "System Configuration", "Manage system-wide settings", CurrentUser);
        ActiveSection = "notification";
        SuccessMessage = "Notification settings saved successfully.";
        return Page();
    }
}
