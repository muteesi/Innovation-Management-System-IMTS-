using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.StaffUser;

public class SubmitIdeaModel : BasePageModel
{
    [BindProperty]
    public string FullName { get; set; } = "";

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Department { get; set; } = "";

    [BindProperty]
    public string JobTitle { get; set; } = "";

    [BindProperty]
    public string Location { get; set; } = "";

    [BindProperty]
    public string IdeaTitle { get; set; } = "";

    [BindProperty]
    public string Summary { get; set; } = "";

    [BindProperty]
    public string ProblemStatement { get; set; } = "";

    [BindProperty]
    public string ProposedSolution { get; set; } = "";

    [BindProperty]
    public List<string> InnovationTypes { get; set; } = new();

    [BindProperty]
    public string Enablers { get; set; } = "";

    [BindProperty]
    public string PotentialImpact { get; set; } = "";

    [BindProperty]
    public string Benefits { get; set; } = "";

    [BindProperty]
    public List<string> StrategicAlignments { get; set; } = new();

    [BindProperty]
    public List<string> SdgAlignments { get; set; } = new();

    [BindProperty]
    public int CurrentStep { get; set; } = 1;

    public string SuccessMessage { get; set; } = "";

    public void OnGet()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Submit Idea", "Submit Innovation Idea", "Share your innovation with the bank", user);

        FullName = user.FullName;
        Department = user.Department;
        CurrentStep = 1;
    }

    public IActionResult OnPost()
    {
        var user = StubData.CurrentStaff;
        SetCommonViewData("Submit Idea", "Submit Innovation Idea", "Share your innovation with the bank", user);

        if (CurrentStep < 4)
        {
            CurrentStep++;
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        TempData["SuccessMessage"] = "Your innovation idea has been submitted successfully!";
        return RedirectToPage("/StaffUser/MyIdeas");
    }
}
