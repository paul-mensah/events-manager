using EventsManager.API.Models.Filters;
using EventsManager.API.Models.Requests;
using EventsManager.API.Models.Responses;

namespace EventsManager.API.Services.Interfaces;

public interface IInvitationService
{
    Task<BaseResponse<EventInvitationResponse>> CreateInvitation(EventInvitationRequest request);
    Task<BaseResponse<EventInvitationResponse>> GetInvitationById(string id);
    Task<BaseResponse<EventInvitationResponse>> DeclineInvitation(string id);
    Task<BaseResponse<EventInvitationResponse>> AcceptInvitation(string id);
    Task<BaseResponse<EmptyResponse>> DeleteInvitation(string id);
    Task<BaseResponse<IEnumerable<EventInvitationResponse>>> GetUserInvitationsPendingApproval(string username);
    Task<BaseResponse<PaginatedResponse<EventInvitationResponse>>> GetEventInvitations(EventInvitationsFilter filter);
}