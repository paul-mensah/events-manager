using System.ComponentModel.DataAnnotations;

namespace EventsManager.API.Models.Requests;

public class EventParticipantRequest
{
    [Required(AllowEmptyStrings = false)] public string Username { get; set; }

    [Required(AllowEmptyStrings = false)]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(AllowEmptyStrings = false)] public string Name { get; set; }

    [Required(AllowEmptyStrings = false)] public string PhotoUrl { get; set; }
}