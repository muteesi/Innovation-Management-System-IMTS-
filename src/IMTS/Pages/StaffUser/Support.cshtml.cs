using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.StaffUser;

public class SupportModel : BasePageModel
{
    [BindProperty]
    public string ContactName { get; set; } = "";

    [BindProperty]
    public string ContactEmail { get; set; } = "";

    [BindProperty]
    public string ContactSubject { get; set; } = "";

    [BindProperty]
    public string ContactMessage { get; set; } = "";

    public string SuccessMessage { get; set; } = "";

    public void OnGet()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Support", "Help & Support", "Get assistance with the Innovation Management System", user);

        ContactName = user.FullName;
        ContactEmail = "jonathan.doe@bankofuganda.org";
    }

    public IActionResult OnPost()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Support", "Help & Support", "Get assistance with the Innovation Management System", user);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        TempData["SuccessMessage"] = "Your support request has been submitted. We will get back to you shortly.";
        return RedirectToPage();
    }
}
