using IMTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IMTS.Pages.InnovationTeam;

public class ReportsAndAnalyticsModel : BasePageModel
{
    public int SubmissionRateValue => 38;
    public string SubmissionRateChange => "+12%";
    public double AvgReviewSpeedDays => 4.8;
    public string AvgReviewSpeedChange => "-0.6";
    public double ApprovalRatio => 31.5;
    public string ApprovalRatioLabel => "31.5%";
    public string PrototypeBudget => "UGX 145M";

    public List<ArchivedReport> ArchivedReports { get; set; } = new();

    public List<CategoryStat> CategoryStats { get; set; } = new();

    public void OnGet()
    {
        var user = StubData.CurrentReviewer;
        SetCommonViewData("Reports & Analytics", "Reports & Analytics Console", "Live key performance indicators and workflow velocity stats", user);

        var ideas = StubData.GetIdeas();
        CategoryStats = ideas
            .GroupBy(i => i.Category)
            .Select(g => new CategoryStat { Category = g.Key, Count = g.Count() })
            .OrderByDescending(c => c.Count)
            .ToList();

        ArchivedReports = new List<ArchivedReport>
        {
            new() { Id = 1, Title = "Q3 2024 Innovation Report", DateGenerated = DateTime.Parse("2024-10-01"), Format = "PDF" },
            new() { Id = 2, Title = "Q3 2024 Innovation Report", DateGenerated = DateTime.Parse("2024-10-01"), Format = "XLSX" },
            new() { Id = 3, Title = "Idea Submission Analytics - Sep 2024", DateGenerated = DateTime.Parse("2024-09-30"), Format = "PDF" },
            new() { Id = 4, Title = "Review Cycle Performance", DateGenerated = DateTime.Parse("2024-09-15"), Format = "PDF" },
            new() { Id = 5, Title = "Category Breakdown Report", DateGenerated = DateTime.Parse("2024-09-01"), Format = "XLSX" }
        };
    }
}

public class ArchivedReport
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime DateGenerated { get; set; }
    public string Format { get; set; } = "";
}

public class CategoryStat
{
    public string Category { get; set; } = "";
    public int Count { get; set; }
}
