using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.InnovationTeam;

public class SettingsModel : BasePageModel
{
    public UserModel CurrentUser { get; set; } = new();

    [BindProperty]
    public string DefaultReviewTimeline { get; set; } = "14";

    [BindProperty]
    public bool NotifyOnNewSubmission { get; set; } = true;

    [BindProperty]
    public bool NotifyOnStatusChange { get; set; } = true;

    [BindProperty]
    public bool NotifyOnComment { get; set; } = false;

    [BindProperty]
    public bool EmailDailyDigest { get; set; } = true;

    [BindProperty]
    public bool EmailWeeklySummary { get; set; } = false;

    [BindProperty]
    public string EmailAddress { get; set; } = "r.namaganda@bou.or.ug";

    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
        CurrentUser = StubData.CurrentReviewer;
        SetCommonViewData("Settings", "Team Settings", "Manage innovation team preferences", CurrentUser);
    }

    public IActionResult OnPost()
    {
        CurrentUser = StubData.CurrentReviewer;
        SetCommonViewData("Settings", "Team Settings", "Manage innovation team preferences", CurrentUser);
        SuccessMessage = "Settings saved successfully.";
        return Page();
    }
}
