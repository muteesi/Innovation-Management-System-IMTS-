using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.InnovationTeam;

public class ManageResourcesModel : BasePageModel
{
    public List<ResourceItem> Resources { get; set; } = new();

    [BindProperty]
    public string NewTitle { get; set; } = "";

    [BindProperty]
    public string NewType { get; set; } = "";

    [BindProperty]
    public string NewDescription { get; set; } = "";

    [BindProperty]
    public int EditResourceId { get; set; }

    [BindProperty]
    public string EditTitle { get; set; } = "";

    [BindProperty]
    public string EditType { get; set; } = "";

    [BindProperty]
    public string EditDescription { get; set; } = "";

    [BindProperty]
    public int DeleteResourceId { get; set; }

    public string? SuccessMessage { get; set; }

    public List<string> ResourceTypes { get; } = new() { "Document", "Video", "Template", "Toolkit", "Guide" };

    public void OnGet()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Manage Resources", "Innovation Resources", "Curate resources for innovators", user);
        LoadResources();
    }

    public IActionResult OnPostAdd()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Manage Resources", "Innovation Resources", "Curate resources for innovators", user);
        LoadResources();
        SuccessMessage = $"Resource '{NewTitle}' uploaded successfully.";
        return Page();
    }

    public IActionResult OnPostEdit()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Manage Resources", "Innovation Resources", "Curate resources for innovators", user);
        LoadResources();
        SuccessMessage = "Resource updated successfully.";
        return Page();
    }

    public IActionResult OnPostDelete()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Manage Resources", "Innovation Resources", "Curate resources for innovators", user);
        LoadResources();
        SuccessMessage = "Resource deleted successfully.";
        return Page();
    }

    private void LoadResources()
    {
        Resources = new List<ResourceItem>
        {
            new() { Id = 1, Title = "Innovation Guidelines", Type = "Document", Description = "Official BOU innovation submission guidelines and criteria.", UploadDate = DateTime.Parse("2024-01-15") },
            new() { Id = 2, Title = "Design Thinking Toolkit", Type = "Toolkit", Description = "A comprehensive toolkit for design thinking workshops.", UploadDate = DateTime.Parse("2024-02-20") },
            new() { Id = 3, Title = "Budget Templates", Type = "Template", Description = "Standard budget proposal templates for innovation projects.", UploadDate = DateTime.Parse("2024-03-10") },
            new() { Id = 4, Title = "Case Studies Book", Type = "Guide", Description = "Collection of successful innovation case studies from central banks.", UploadDate = DateTime.Parse("2024-04-05") }
        };
    }
}

public class ResourceItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime UploadDate { get; set; }
}
