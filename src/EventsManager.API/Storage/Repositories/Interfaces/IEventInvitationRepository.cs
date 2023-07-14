using EventsManager.API.Models.Filters;
using EventsManager.API.Models.Responses;
using EventsManager.API.Storage.Domain.EventInvitations;

namespace EventsManager.API.Storage.Repositories.Interfaces;

public interface IEventInvitationRepository : IRepositoryBase<EventInvitation>
{
    Task<PaginatedResponse<EventInvitation>> GetEventInvitations(EventInvitationsFilter filter);
    Task<List<EventInvitation>> GetUserInvitationsPendingApproval(string username);
}