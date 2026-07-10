using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.InnovationTeam;

public class ReviewIdeasModel : BasePageModel
{
    public List<IdeaModel> Ideas { get; set; } = new();
    public List<string> Categories { get; set; } = new();

    [BindProperty]
    public string? SelectedCategory { get; set; }

    [BindProperty]
    public int ReviewIdeaId { get; set; }

    [BindProperty]
    public string ReviewDecision { get; set; } = "";

    [BindProperty]
    public string ReviewComments { get; set; } = "";

    [BindProperty]
    public int ReviewScore { get; set; }

    [BindProperty]
    public List<int> SelectedIds { get; set; } = new();

    [BindProperty]
    public string BulkAction { get; set; } = "";

    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Review Ideas", "Review Ideas", "Evaluate and provide feedback on submissions", user);
        Ideas = StubData.GetIdeas().Where(i => i.Status is "Under Review" or "Pending" or "In Review").ToList();
        Categories = Ideas.Select(i => i.Category).Distinct().ToList();
    }

    public IActionResult OnPost()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Review Ideas", "Review Ideas", "Evaluate and provide feedback on submissions", user);
        Ideas = StubData.GetIdeas().Where(i => i.Status is "Under Review" or "Pending" or "In Review").ToList();
        Categories = Ideas.Select(i => i.Category).Distinct().ToList();
        SuccessMessage = $"Idea #{ReviewIdeaId} has been reviewed with decision: {ReviewDecision}.";
        return Page();
    }

    public IActionResult OnPostBulk()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Review Ideas", "Review Ideas", "Evaluate and provide feedback on submissions", user);
        Ideas = StubData.GetIdeas().Where(i => i.Status is "Under Review" or "Pending" or "In Review").ToList();
        Categories = Ideas.Select(i => i.Category).Distinct().ToList();
        SuccessMessage = $"{SelectedIds.Count} idea(s) {BulkAction} successfully.";
        return Page();
    }
}
