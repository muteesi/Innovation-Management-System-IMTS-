using IMTS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.StaffUser;

public class DashboardModel : BasePageModel
{
    public int TotalSubmitted { get; set; }
    public int UnderReview { get; set; }
    public int Approved { get; set; }
    public int InDevelopment { get; set; }
    public List<IdeaModel> RecentIdeas { get; set; } = new();

    public void OnGet()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Staff Dashboard", "Welcome Back, " + user.FullName, DateTime.Now.ToString("dddd, dd MMMM yyyy"), user);

        var ideas = StubData.GetIdeas();
        TotalSubmitted = 12;
        UnderReview = ideas.Count(i => i.Status == "Under Review");
        Approved = ideas.Count(i => i.Status == "Approved");
        InDevelopment = ideas.Count(i => i.Status == "In Development");
        RecentIdeas = ideas.Where(i => i.Submitter == user.FullName).Take(4).ToList();
    }
}
