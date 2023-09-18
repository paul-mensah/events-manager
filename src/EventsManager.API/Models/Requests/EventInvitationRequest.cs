using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace EventsManager.API.Models.Requests;

public class EventInvitationRequest
{
    [Required(AllowEmptyStrings = false)] public string Username { get; set; }

    [Required(AllowEmptyStrings = false)]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(AllowEmptyStrings = false)] public string Name { get; set; }

    [Required(AllowEmptyStrings = false)] public string PhotoUrl { get; set; }

    [Required(AllowEmptyStrings = false)] public string EventId { get; set; }

    [Required(AllowEmptyStrings = false)] public string InvitedBy { get; set; }
}

public class EventInvitationRequestValidator : AbstractValidator<EventInvitationRequest>
{
    public EventInvitationRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x != null &&
                       !string.IsNullOrEmpty(x.Username) &&
                       !string.IsNullOrEmpty(x.Email) &&
                       !string.IsNullOrEmpty(x.Name) &&
                       !string.IsNullOrEmpty(x.PhotoUrl) &&
                       !string.IsNullOrEmpty(x.EventId) &&
                       !string.IsNullOrEmpty(x.InvitedBy));
    }
}