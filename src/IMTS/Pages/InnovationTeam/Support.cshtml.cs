using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.InnovationTeam;

public class SupportModel : BasePageModel
{
    [BindProperty]
    public string ContactName { get; set; } = "";

    [BindProperty]
    public string ContactEmail { get; set; } = "";

    [BindProperty]
    public string ContactSubject { get; set; } = "";

    [BindProperty]
    public string ContactMessage { get; set; } = "";

    public string? SuccessMessage { get; set; }

    public UserModel CurrentUser { get; set; } = new();

    public List<FaqItem> FaqItems { get; set; } = new();

    public void OnGet()
    {
        CurrentUser = StubData.CurrentReviewer;
        SetCommonViewData("Support", "Help & Support", "Get assistance", CurrentUser);
        LoadFaq();
    }

    public IActionResult OnPost()
    {
        CurrentUser = StubData.CurrentReviewer;
        SetCommonViewData("Support", "Help & Support", "Get assistance", CurrentUser);
        LoadFaq();
        SuccessMessage = "Your message has been sent. We will get back to you within 24 hours.";
        return Page();
    }

    private void LoadFaq()
    {
        FaqItems = new List<FaqItem>
        {
            new() { Id = 1, Question = "How do I review an idea?", Answer = "Navigate to the Review Ideas page, select an idea card, and click the Review button. Fill in your decision, score, and comments then submit." },
            new() { Id = 2, Question = "What are the review criteria?", Answer = "Ideas are evaluated based on innovation, feasibility, impact, alignment with BOU goals, and scalability." },
            new() { Id = 3, Question = "How do I manage categories?", Answer = "Go to the Manage Categories page where you can add, edit, or delete categories used to organize submissions." },
            new() { Id = 4, Question = "How do I set review timelines?", Answer = "Use the Set Timelines page to create and manage review cycles, deadlines, and milestone phases." },
            new() { Id = 5, Question = "How do I generate reports?", Answer = "Visit the Reports & Analytics page to view KPIs, charts, and download archived reports in PDF or XLSX format." }
        };
    }
}

public class FaqItem
{
    public int Id { get; set; }
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
}
