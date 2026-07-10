using IMTS.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.StaffUser;

public class MyIdeasModel : BasePageModel
{
    public List<IdeaModel> Ideas { get; set; } = new();
    public string SearchTerm { get; set; } = "";

    public void OnGet(string search)
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("My Ideas", "My Innovation Ideas", "Manage your submitted ideas", user);

        SearchTerm = search ?? "";
        var allIdeas = StubData.GetIdeas();
        Ideas = allIdeas.Where(i => i.Submitter == user.FullName).ToList();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var s = SearchTerm.ToLower();
            Ideas = Ideas.Where(i =>
                i.Title.ToLower().Contains(s) ||
                i.Category.ToLower().Contains(s) ||
                i.Status.ToLower().Contains(s)).ToList();
        }
    }
}
