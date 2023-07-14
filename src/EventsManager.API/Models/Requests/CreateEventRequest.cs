using System.ComponentModel.DataAnnotations;
using EventsManager.API.Storage.Domain.Events;
using FluentValidation;

namespace EventsManager.API.Models.Requests;

public class CreateEventRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string Description { get; set; }
    [Required(AllowEmptyStrings = false), DataType(DataType.Url)]
    public string PhotoUrl { get; set; }
    [Required]
    public EventLocation Location { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string CreatedBy { get; set; }
}

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x != null &&
                       !string.IsNullOrEmpty(x.Title) &&
                       !string.IsNullOrEmpty(x.Description) &&
                       !string.IsNullOrEmpty(x.PhotoUrl) &&
                       x.Location != null &&
                       !string.IsNullOrEmpty(x.Location.ZipCode) &&
                       !string.IsNullOrEmpty(x.Location.City) &&
                       x.EndDate > DateTime.UtcNow &&
                       !string.IsNullOrEmpty(x.CreatedBy));
    }
}

