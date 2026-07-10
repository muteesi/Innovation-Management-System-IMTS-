using IMTS.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMTS.Pages.ItAdmin;

public class ManageAccountsModel : BasePageModel
{
    public UserModel CurrentUser => StubData.CurrentAdmin;

    [BindProperty]
    public string SearchTerm { get; set; } = "";

    [BindProperty]
    public string DepartmentFilter { get; set; } = "";

    [BindProperty]
    public string RoleFilter { get; set; } = "";

    [BindProperty]
    public string NewUsername { get; set; } = "";

    [BindProperty]
    public string NewFullName { get; set; } = "";

    [BindProperty]
    public string NewEmail { get; set; } = "";

    [BindProperty]
    public string NewDepartment { get; set; } = "";

    [BindProperty]
    public string NewRole { get; set; } = "Staff";

    [BindProperty]
    public string NewPassword { get; set; } = "";

    [BindProperty]
    public string BulkAction { get; set; } = "";

    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string SuccessMessage { get; set; } = "";
    public string ErrorMessage { get; set; } = "";

    public List<AccountUser> Users { get; set; } = new();
    public List<AccountUser> FilteredUsers { get; set; } = new();
    public List<AccountUser> PagedUsers { get; set; } = new();

    public void OnGet(int? page)
    {
        SetCommonViewData("Manage Accounts", "Account Management", "Create, edit, and manage user accounts", CurrentUser);
        CurrentPage = page ?? 1;
        LoadUsers();
    }

    public IActionResult OnPostSearch()
    {
        SetCommonViewData("Manage Accounts", "Account Management", "Create, edit, and manage user accounts", CurrentUser);
        CurrentPage = 1;
        LoadUsers();
        return Page();
    }

    public IActionResult OnPostCreate()
    {
        SetCommonViewData("Manage Accounts", "Account Management", "Create, edit, and manage user accounts", CurrentUser);
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewFullName))
        {
            ErrorMessage = "Username and Full Name are required.";
            LoadUsers();
            return Page();
        }
        SuccessMessage = $"User '{NewFullName}' created successfully. Initial password set.";
        NewUsername = NewFullName = NewEmail = NewDepartment = NewPassword = "";
        NewRole = "Staff";
        LoadUsers();
        return Page();
    }

    public IActionResult OnPostBulkAction()
    {
        SetCommonViewData("Manage Accounts", "Account Management", "Create, edit, and manage user accounts", CurrentUser);
        LoadUsers();
        if (!string.IsNullOrEmpty(BulkAction))
        {
            SuccessMessage = $"Bulk action '{BulkAction}' executed successfully.";
        }
        return Page();
    }

    private void LoadUsers()
    {
        Users = new()
        {
            new() { Username = "j.doe", FullName = "Jonathan Doe", Email = "j.doe@bou.or.ug", Role = "Staff", Department = "Information Technology", Status = "Active", LastLogin = DateTime.Today.AddHours(-3) },
            new() { Username = "r.namaganda", FullName = "Rose Namaganda", Email = "r.namaganda@bou.or.ug", Role = "Reviewer", Department = "Innovation Team", Status = "Active", LastLogin = DateTime.Today.AddHours(-1) },
            new() { Username = "m.okello", FullName = "Michael Okello", Email = "m.okello@bou.or.ug", Role = "Staff", Department = "Operations", Status = "Active", LastLogin = DateTime.Today.AddDays(-1) },
            new() { Username = "s.namono", FullName = "Sarah Namono", Email = "s.namono@bou.or.ug", Role = "Staff", Department = "Innovation Team", Status = "Active", LastLogin = DateTime.Today.AddDays(-2) },
            new() { Username = "p.mukasa", FullName = "Peter Mukasa", Email = "p.mukasa@bou.or.ug", Role = "Reviewer", Department = "IT Security", Status = "Locked", LastLogin = DateTime.Today.AddDays(-7) },
            new() { Username = "g.nakato", FullName = "Grace Nakato", Email = "g.nakato@bou.or.ug", Role = "Staff", Department = "Human Resources", Status = "Disabled", LastLogin = DateTime.Today.AddDays(-30) },
            new() { Username = "k.okello", FullName = "Kenneth Okello", Email = "k.okello@bou.or.ug", Role = "Staff", Department = "Legal & Compliance", Status = "Active", LastLogin = DateTime.Today.AddHours(-5) },
            new() { Username = "l.atwine", FullName = "Lydia Atwine", Email = "l.atwine@bou.or.ug", Role = "Staff", Department = "Human Resources", Status = "Active", LastLogin = DateTime.Today.AddDays(-1) }
        };

        var query = Users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var term = SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.FullName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(DepartmentFilter))
            query = query.Where(u => u.Department == DepartmentFilter);

        if (!string.IsNullOrWhiteSpace(RoleFilter))
            query = query.Where(u => u.Role == RoleFilter);

        FilteredUsers = query.ToList();

        TotalPages = (int)Math.Ceiling(FilteredUsers.Count / (double)PageSize);
        if (TotalPages < 1) TotalPages = 1;
        if (CurrentPage < 1) CurrentPage = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        PagedUsers = FilteredUsers.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
    }

    public List<string> AllDepartments => new()
    {
        "Information Technology", "Innovation Team", "Operations", "IT Security",
        "Human Resources", "Legal & Compliance", "Monetary Policy", "Risk Management"
    };

    public List<string> AllRoles => new() { "Staff", "Reviewer", "ITAdmin" };
}

public class AccountUser
{
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string Department { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime LastLogin { get; set; }
}
