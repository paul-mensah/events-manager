namespace EventsManager.API.Models.Filters;

public class EventsFilter : BaseFilter
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string ParticipantUsername { get; set; }
    public string ParticipantName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string CreatedBy { get; set; }
}