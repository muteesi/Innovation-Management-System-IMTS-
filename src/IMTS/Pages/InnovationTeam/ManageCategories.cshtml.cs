using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.InnovationTeam;

public class ManageCategoriesModel : BasePageModel
{
    public List<CategoryItem> Categories { get; set; } = new();

    [BindProperty]
    public string NewCategoryName { get; set; } = "";

    [BindProperty]
    public string NewCategoryDescription { get; set; } = "";

    [BindProperty]
    public string NewCategoryIcon { get; set; } = "";

    [BindProperty]
    public int EditCategoryId { get; set; }

    [BindProperty]
    public string EditCategoryName { get; set; } = "";

    [BindProperty]
    public string EditCategoryDescription { get; set; } = "";

    [BindProperty]
    public string EditCategoryIcon { get; set; } = "";

    [BindProperty]
    public int DeleteCategoryId { get; set; }

    public string? SuccessMessage { get; set; }

    public List<string> AvailableIcons { get; set; } = new()
    {
        "account_balance", "security", "policy", "settings", "lightbulb", "shield"
    };

    public void OnGet()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Manage Categories", "Innovation Categories", "Organize and manage idea categories", user);
        LoadCategories();
    }

    public IActionResult OnPostAdd()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Manage Categories", "Innovation Categories", "Organize and manage idea categories", user);
        LoadCategories();
        SuccessMessage = $"Category '{NewCategoryName}' added successfully.";
        return Page();
    }

    public IActionResult OnPostEdit()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Manage Categories", "Innovation Categories", "Organize and manage idea categories", user);
        LoadCategories();
        SuccessMessage = $"Category updated successfully.";
        return Page();
    }

    public IActionResult OnPostDelete()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Manage Categories", "Innovation Categories", "Organize and manage idea categories", user);
        LoadCategories();
        SuccessMessage = $"Category deleted successfully.";
        return Page();
    }

    private void LoadCategories()
    {
        var ideas = StubData.GetIdeas();
        Categories = new List<CategoryItem>
        {
            new() { Id = 1, Name = "FinTech Solutions", Description = "Financial technology innovations", Icon = "account_balance", IdeaCount = ideas.Count(i => i.Category == "FinTech Solutions") },
            new() { Id = 2, Name = "Security", Description = "Security and fraud prevention", Icon = "security", IdeaCount = ideas.Count(i => i.Category == "Security") },
            new() { Id = 3, Name = "Policy", Description = "Regulatory and policy innovations", Icon = "policy", IdeaCount = ideas.Count(i => i.Category == "Policy") },
            new() { Id = 4, Name = "Operations", Description = "Operational efficiency improvements", Icon = "settings", IdeaCount = ideas.Count(i => i.Category == "Operations") },
            new() { Id = 5, Name = "Innovation", Description = "General innovation ideas", Icon = "lightbulb", IdeaCount = ideas.Count(i => i.Category == "Innovation") },
            new() { Id = 6, Name = "Risk Management", Description = "Risk assessment and management", Icon = "shield", IdeaCount = ideas.Count(i => i.Category == "Risk Management") },
            new() { Id = 7, Name = "Public Policy", Description = "Public policy related ideas", Icon = "policy", IdeaCount = ideas.Count(i => i.Category == "Public Policy") },
            new() { Id = 8, Name = "FinTech", Description = "Financial technology", Icon = "account_balance", IdeaCount = ideas.Count(i => i.Category == "FinTech") }
        };
    }
}

public class CategoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public int IdeaCount { get; set; }
}
