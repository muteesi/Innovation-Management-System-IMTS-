using System.Security.Claims;
using IMTS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public string ErrorMessage { get; set; } = "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required.";
            return Page();
        }

        var username = Username.Trim().ToLower();
        var (role, fullName) = username switch
        {
            var u when u.Contains("staff") || u.Contains("jonathan") || u.Contains("john") || u.Contains("j.doe") => ("Staff", "Jonathan Doe"),
            var u when u.Contains("admin") || u.Contains("katumba") || u.Contains("j.katumba") => ("ITAdmin", "Joseph Katumba"),
            var u when u.Contains("reviewer") || u.Contains("rose") || u.Contains("r.namaganda") || u.Contains("namaganda") => ("Reviewer", "Rose Namaganda"),
            _ => ("", "")
        };

        if (string.IsNullOrEmpty(role))
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.GivenName, fullName),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var route = StubData.GetRoleRoutes()[role];
        return RedirectToPage(route);
    }

    public async Task<IActionResult> OnGetLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}
