using EventsManager.API.Models.Filters;
using EventsManager.API.Models.Requests;
using EventsManager.API.Models.Responses;
using EventsManager.API.Services.Interfaces;
using EventsManager.API.Storage.Domain.Events;
using FluentValidation.Results;
using Mapster;
using Nest;

namespace EventsManager.API.Services.Implementations;

public sealed class EventService : IEventService
{
    private readonly IElasticsearchService _elasticsearchService;

    public EventService(IElasticsearchService elasticsearchService)
    {
        _elasticsearchService = elasticsearchService;
    }

    public async Task<BaseResponse<PaginatedResponse<EventResponse>>> GetEvents(EventsFilter filter)
    {
        QueryBase searchQuery = new TermQuery
        {
            Field = "participants.username.keyword",
            Value = filter.ParticipantUsername
        } && new TermQuery
        {
            Field = "createdBy.keyword",
            Value = filter.CreatedBy
        } && new TermQuery
        {
            Field = "location.zipCode.keyword",
            Value = filter.ZipCode
        } && new DateRangeQuery
        {
            Field = "startDate",
            GreaterThanOrEqualTo = filter.StartDate
        } && new DateRangeQuery
        {
            Field = "endDate",
            LessThanOrEqualTo = filter.EndDate
        };

        if (!string.IsNullOrEmpty(filter.City))
            searchQuery = searchQuery && new MatchQuery
            {
                Field = "location.City",
                Query = filter.City
            };

        if (!string.IsNullOrEmpty(filter.ParticipantName))
            searchQuery = searchQuery && new WildcardQuery
            {
                Field = "participants.name",
                Value = $"*{filter.ParticipantName}*"
            };

        if (!string.IsNullOrEmpty(filter.Title))
            searchQuery = searchQuery && new MatchQuery
            {
                Field = "title",
                Query = filter.Title
            };

        if (!string.IsNullOrEmpty(filter.Description))
            searchQuery = searchQuery && new MatchQuery
            {
                Field = "description",
                Query = filter.Description
            };

        var searchResponse = await _elasticsearchService.SearchAsync(new SearchRequest<Event>
        {
            Query = searchQuery,
            Size = filter.PageSize,
            From = (filter.CurrentPage - 1) * filter.PageSize,
            Sort = new List<ISort>
            {
                new FieldSort
                {
                    Field = "createdAt",
                    Order = "desc".Equals(filter.OrderBy, StringComparison.OrdinalIgnoreCase)
                        ? SortOrder.Descending
                        : SortOrder.Ascending
                }
            }
        });

        if (!searchResponse.IsValid)
            return CommonResponses.ErrorResponse.FailedDependencyErrorResponse<PaginatedResponse<EventResponse>>();

        var eventResponseList = searchResponse.Documents.Adapt<List<EventResponse>>();
        var response = new PaginatedResponse<EventResponse>
        {
            Data = eventResponseList,
            PageSize = filter.PageSize,
            CurrentPage = filter.CurrentPage,
            TotalRecords = (int)searchResponse.Total,
            TotalPages = (int)Math.Ceiling((decimal)searchResponse.Total / filter.PageSize)
        };

        return CommonResponses.SuccessResponse.OkResponse(response);
    }

    public async Task<BaseResponse<EventResponse>> GetEventById(string id)
    {
        Event @event = await _elasticsearchService.GetByIdAsync<Event>(id);

        return @event == null
            ? CommonResponses.ErrorResponse.NotFoundErrorResponse<EventResponse>("Event not found")
            : CommonResponses.SuccessResponse.OkResponse(@event.Adapt<EventResponse>());
    }

    public async Task<BaseResponse<EmptyResponse>> DeleteEventById(string id)
    {
        bool isDeleted = await _elasticsearchService.DeleteAsync<EventResponse>(id);

        return isDeleted
            ? CommonResponses.SuccessResponse.DeletedResponse()
            : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EmptyResponse>();
    }

    public async Task<BaseResponse<EventResponse>> CreateEvent(CreateEventRequest request)
    {
        ValidationResult validateResponse = await new CreateEventRequestValidator().ValidateAsync(request);

        if (!validateResponse.IsValid)
            return CommonResponses.ErrorResponse.BadRequestResponse<EventResponse>("Provide all required inputs");

        Event newEvent = request.Adapt<Event>();
        bool isCreated = await _elasticsearchService.AddAsync(newEvent);

        return isCreated
            ? CommonResponses.SuccessResponse.CreatedResponse(newEvent.Adapt<EventResponse>())
            : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EventResponse>();
    }

    public async Task<BaseResponse<EventResponse>> AddParticipant(string id, EventParticipantRequest participant)
    {
        Event @event = await _elasticsearchService.GetByIdAsync<Event>(id);

        if (@event is null)
            return CommonResponses.ErrorResponse.NotFoundErrorResponse<EventResponse>("Event not found");

        bool userAlreadyAdded = @event.Participants.Any(x =>
            x.Username.ToLower().Equals(participant.Username.ToLower()) ||
            x.Email.ToLower().Equals(participant.Email.ToLower()));

        if (userAlreadyAdded)
            return CommonResponses.ErrorResponse
                .ConflictErrorResponse<EventResponse>("User already added as participant");

        @event.Participants.Add(participant.Adapt<EventParticipant>());
        @event.UpdatedAt = DateTime.UtcNow;

        bool isUpdated = await _elasticsearchService.UpdateAsync(id, @event);

        return isUpdated
            ? CommonResponses.SuccessResponse
                .OkResponse(@event.Adapt<EventResponse>(), "Participant added successfully")
            : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EventResponse>();
    }

    public async Task<BaseResponse<EventResponse>> RemoveParticipant(string id, string username)
    {
        Event @event = await _elasticsearchService.GetByIdAsync<Event>(id);

        if (@event is null)
            return CommonResponses.ErrorResponse.NotFoundErrorResponse<EventResponse>("Event not found");

        EventParticipant eventParticipant = @event.Participants.FirstOrDefault(x => x.Username.Equals(username));

        if (eventParticipant is null)
            return CommonResponses.ErrorResponse
                .NotFoundErrorResponse<EventResponse>("User is not part of event participants");

        @event.Participants.Remove(eventParticipant);
        @event.UpdatedAt = DateTime.UtcNow;

        bool isUpdated = await _elasticsearchService.UpdateAsync(id, @event);

        return isUpdated
            ? CommonResponses.SuccessResponse
                .OkResponse(@event.Adapt<EventResponse>(), "Participant added successfully")
            : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EventResponse>();
    }
}