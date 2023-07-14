using EventsManager.API.Models.Filters;
using EventsManager.API.Models.Requests;
using EventsManager.API.Models.Responses;

namespace EventsManager.API.Services.Interfaces;

public interface IEventService
{
    Task<BaseResponse<PaginatedResponse<EventResponse>>> GetEvents(EventsFilter filter);
    Task<BaseResponse<EventResponse>> GetEventById(string id);
    Task<BaseResponse<EmptyResponse>> DeleteEventById(string id);
    Task<BaseResponse<EventResponse>> CreateEvent(CreateEventRequest request);
    Task<BaseResponse<EventResponse>> AddParticipant(string id, EventParticipantRequest participant);
    Task<BaseResponse<EventResponse>> RemoveParticipant(string id, string username);
}