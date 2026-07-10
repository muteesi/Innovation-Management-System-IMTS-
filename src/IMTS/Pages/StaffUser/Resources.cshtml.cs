using IMTS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.StaffUser;

public class ResourcesModel : BasePageModel
{
    public void OnGet()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Resources", "Innovation Resources", "Tools and guides to help you innovate", user);
    }
}
