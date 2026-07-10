using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.StaffUser;

public class IdeaDetailsModel : BasePageModel
{
    public IdeaModel? Idea { get; set; }

    public IActionResult OnGet(int id)
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Idea Details", "Idea Details", "Full view of your innovation idea", user);

        Idea = StubData.GetIdeas().FirstOrDefault(i => i.Id == id);
        if (Idea == null)
        {
            return NotFound();
        }

        return Page();
    }
}
