using IMTS.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMTS.Pages;

public class IndexModel : BasePageModel
{
    public IActionResult OnGet()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToPage("/Login");

        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        var route = StubData.GetRoleRoutes().GetValueOrDefault(role, "/Login");
        return RedirectToPage(route);
    }
}
