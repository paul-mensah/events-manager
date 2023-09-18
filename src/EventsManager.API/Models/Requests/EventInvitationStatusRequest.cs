using System.ComponentModel.DataAnnotations;

namespace EventsManager.API.Models.Requests;

public class EventInvitationStatusRequest
{
    [Required(AllowEmptyStrings = false),
    RegularExpression("accept|decline")]
    public string Status { get; set; }
}