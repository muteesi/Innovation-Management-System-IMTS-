using IMTS.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMTS.Pages.ItAdmin;

public class SupportModel : BasePageModel
{
    public UserModel CurrentUser => StubData.CurrentAdmin;

    [BindProperty]
    public string Subject { get; set; } = "";

    [BindProperty]
    public string Message { get; set; } = "";

    [BindProperty]
    public string Priority { get; set; } = "Normal";

    public string SuccessMessage { get; set; } = "";

    public List<SupportTicket> Tickets { get; set; } = new();

    public void OnGet()
    {
        SetCommonViewData("Support", "IT Support", "Manage support tickets and inquiries", CurrentUser);
        LoadTickets();
    }

    public IActionResult OnPostSubmit()
    {
        SetCommonViewData("Support", "IT Support", "Manage support tickets and inquiries", CurrentUser);
        LoadTickets();

        if (string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Message))
        {
            ModelState.AddModelError("", "Subject and message are required.");
            return Page();
        }

        SuccessMessage = "Your support request has been submitted successfully.";
        Tickets.Insert(0, new()
        {
            Id = Tickets.Count + 1,
            Subject = Subject,
            SubmittedBy = CurrentUser.FullName,
            Department = CurrentUser.Department,
            Priority = Priority,
            Status = "Open",
            SubmittedAt = DateTime.Now,
            LastUpdated = DateTime.Now
        });

        Subject = "";
        Message = "";
        Priority = "Normal";
        return Page();
    }

    public IActionResult OnPostUpdateStatus(int ticketId, string status)
    {
        SetCommonViewData("Support", "IT Support", "Manage support tickets and inquiries", CurrentUser);
        LoadTickets();
        var ticket = Tickets.FirstOrDefault(t => t.Id == ticketId);
        if (ticket != null)
        {
            ticket.Status = status;
            ticket.LastUpdated = DateTime.Now;
            SuccessMessage = $"Ticket #{ticketId} status updated to '{status}'.";
        }
        return Page();
    }

    private void LoadTickets()
    {
        Tickets = new()
        {
            new() { Id = 1, Subject = "User unable to reset password", SubmittedBy = "Rose Namaganda", Department = "Innovation Team", Priority = "High", Status = "In Progress", SubmittedAt = DateTime.Today.AddHours(-5), LastUpdated = DateTime.Today.AddHours(-2) },
            new() { Id = 2, Subject = "New staff onboarding - account creation", SubmittedBy = "HR Department", Department = "Human Resources", Priority = "Normal", Status = "Open", SubmittedAt = DateTime.Today.AddDays(-1), LastUpdated = DateTime.Today.AddDays(-1) },
            new() { Id = 3, Subject = "System access request for external auditor", SubmittedBy = "Jonathan Doe", Department = "Information Technology", Priority = "Low", Status = "Open", SubmittedAt = DateTime.Today.AddDays(-2), LastUpdated = DateTime.Today.AddDays(-1) },
            new() { Id = 4, Subject = "Two-factor authentication enrollment issue", SubmittedBy = "Michael Okello", Department = "Operations", Priority = "High", Status = "Resolved", SubmittedAt = DateTime.Today.AddDays(-3), LastUpdated = DateTime.Today.AddDays(-1) },
            new() { Id = 5, Subject = "Role change request - Staff to Reviewer", SubmittedBy = "Sarah Namono", Department = "Innovation Team", Priority = "Normal", Status = "Closed", SubmittedAt = DateTime.Today.AddDays(-5), LastUpdated = DateTime.Today.AddDays(-4) }
        };
    }
}

public class SupportTicket
{
    public int Id { get; set; }
    public string Subject { get; set; } = "";
    public string SubmittedBy { get; set; } = "";
    public string Department { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime SubmittedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}
