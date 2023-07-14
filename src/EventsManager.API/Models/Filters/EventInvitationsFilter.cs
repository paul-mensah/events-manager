namespace EventsManager.API.Models.Filters;

public class EventInvitationsFilter : BaseFilter
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string EventId { get; set; }
    public string Title { get; set; }
    public string InvitedBy { get; set; }
    public bool? IsAccepted { get; set; }
    public DateTime? AcceptedDate { get; set; }
}