using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.InnovationTeam;

public class DashboardModel : BasePageModel
{
    public List<IdeaModel> Ideas { get; set; } = new();
    public int TotalIdeas => 45;
    public int PendingReview => 12;
    public int InDevelopment => 8;
    public int Deployed => 5;
    public int Approved => 15;
    public int Declined => 5;

    public void OnGet()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Innovation Team Dashboard", "Innovation Management System", "Review and manage submissions", user);
        Ideas = StubData.GetIdeas();
    }
}
