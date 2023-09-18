using Nest;

namespace EventsManager.API.Storage.Domain.Events;

public class Event : EntityBase
{
    [Text] public string Title { get; set; }

    [Text] public string Description { get; set; }

    public string PhotoUrl { get; set; }
    public EventLocation Location { get; set; }
    public List<EventParticipant> Participants { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [Keyword] public string CreatedBy { get; set; }
}