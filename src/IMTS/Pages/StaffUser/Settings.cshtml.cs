using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.StaffUser;

public class SettingsModel : BasePageModel
{
    [BindProperty]
    public string FullName { get; set; } = "";

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Department { get; set; } = "";

    [BindProperty]
    public bool NotifyStatusChanges { get; set; } = true;

    [BindProperty]
    public bool NotifyComments { get; set; } = true;

    [BindProperty]
    public bool NotifyReminders { get; set; } = true;

    [BindProperty]
    public string CurrentPassword { get; set; } = "";

    [BindProperty]
    public string NewPassword { get; set; } = "";

    [BindProperty]
    public string ConfirmPassword { get; set; } = "";

    public string SuccessMessage { get; set; } = "";

    public void OnGet()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Settings", "Account Settings", "Manage your profile and preferences", user);

        FullName = user.FullName;
        Email = "jonathan.doe@bankofuganda.org";
        Department = user.Department;
    }

    public IActionResult OnPost()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Settings", "Account Settings", "Manage your profile and preferences", user);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        TempData["SuccessMessage"] = "Your settings have been saved successfully.";
        return RedirectToPage();
    }
}
