using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.StaffUser;

public class NotificationsModel : BasePageModel
{
    public List<NotificationModel> Notifications { get; set; } = new();

    public void OnGet()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Notifications", "Notifications", "Stay updated on your ideas", user);

        Notifications = StubData.GetNotifications().OrderByDescending(n => n.CreatedAt).ToList();
    }

    public IActionResult OnPostMarkAllRead()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Notifications", "Notifications", "Stay updated on your ideas", user);

        TempData["SuccessMessage"] = "All notifications marked as read.";
        return RedirectToPage();
    }
}
