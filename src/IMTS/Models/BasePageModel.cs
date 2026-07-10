using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Models;

public abstract class BasePageModel : PageModel
{
    public virtual void SetCommonViewData(string title, string heading, string subheading, UserModel user)
    {
        ViewData["Title"] = title;
        ViewData["Heading"] = heading;
        ViewData["Subheading"] = subheading;
        ViewData["Role"] = user.Role;
        ViewData["AvatarUrl"] = user.AvatarUrl;
        ViewData["FullName"] = user.FullName;
    }
}
