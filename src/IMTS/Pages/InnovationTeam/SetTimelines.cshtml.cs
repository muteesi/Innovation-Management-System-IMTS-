using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.InnovationTeam;

public class SetTimelinesModel : BasePageModel
{
    public List<TimelineItem> Timelines { get; set; } = new();

    [BindProperty]
    public string NewTitle { get; set; } = "";

    [BindProperty]
    public string NewDescription { get; set; } = "";

    [BindProperty]
    public DateTime NewStartDate { get; set; } = DateTime.Today;

    [BindProperty]
    public DateTime NewEndDate { get; set; } = DateTime.Today.AddDays(30);

    [BindProperty]
    public string NewPhase { get; set; } = "";

    [BindProperty]
    public int EditTimelineId { get; set; }

    [BindProperty]
    public string EditTitle { get; set; } = "";

    [BindProperty]
    public string EditDescription { get; set; } = "";

    [BindProperty]
    public DateTime EditStartDate { get; set; }

    [BindProperty]
    public DateTime EditEndDate { get; set; }

    [BindProperty]
    public string EditPhase { get; set; } = "";

    [BindProperty]
    public int DeleteTimelineId { get; set; }

    public string? SuccessMessage { get; set; }

    public List<string> Phases { get; } = new() { "Submission", "Review", "Development", "Testing", "Deployment", "Demo" };

    public void OnGet()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Set Timelines", "Innovation Timelines", "Manage review cycles and deadlines", user);
        LoadTimelines();
    }

    public IActionResult OnPostAdd()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Set Timelines", "Innovation Timelines", "Manage review cycles and deadlines", user);
        LoadTimelines();
        SuccessMessage = $"Timeline '{NewTitle}' added successfully.";
        return Page();
    }

    public IActionResult OnPostEdit()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Set Timelines", "Innovation Timelines", "Manage review cycles and deadlines", user);
        LoadTimelines();
        SuccessMessage = "Timeline updated successfully.";
        return Page();
    }

    public IActionResult OnPostDelete()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Set Timelines", "Innovation Timelines", "Manage review cycles and deadlines", user);
        LoadTimelines();
        SuccessMessage = "Timeline deleted successfully.";
        return Page();
    }

    private void LoadTimelines()
    {
        Timelines = new List<TimelineItem>
        {
            new() { Id = 1, Title = "Q4 Idea Cutoff", Description = "Final submission deadline for Q4 innovation ideas", StartDate = DateTime.Parse("2024-10-01"), EndDate = DateTime.Parse("2024-10-25"), Phase = "Submission" },
            new() { Id = 2, Title = "Panel Review", Description = "Review panel evaluates submitted ideas", StartDate = DateTime.Parse("2024-10-28"), EndDate = DateTime.Parse("2024-11-02"), Phase = "Review" },
            new() { Id = 3, Title = "Prototype Submission", Description = "Deadline for approved idea prototypes", StartDate = DateTime.Parse("2024-11-05"), EndDate = DateTime.Parse("2024-12-01"), Phase = "Development" },
            new() { Id = 4, Title = "Demo Day", Description = "Final presentation and demo of completed projects", StartDate = DateTime.Parse("2024-12-10"), EndDate = DateTime.Parse("2024-12-15"), Phase = "Demo" }
        };
    }
}

public class TimelineItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Phase { get; set; } = "";
}
