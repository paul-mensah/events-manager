namespace EventsManager.API.Storage.Domain.Events;

public class CachedEventInvitation
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string PhotoUrl { get; set; }
    public string EventId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string InvitedBy { get; set; }
    public bool IsAccepted { get; set; } = false;
    public DateTime? AcceptedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}