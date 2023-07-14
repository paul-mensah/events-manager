using EventsManager.API.Models.Filters;
using EventsManager.API.Models.Requests;
using EventsManager.API.Models.Responses;
using EventsManager.API.Services.Interfaces;
using EventsManager.API.Storage.Domain.Events;
using FluentValidation.Results;
using Mapster;
using Nest;
using Newtonsoft.Json;
using EventResponse = EventsManager.API.Models.Responses.EventResponse;
using TermQuery = Nest.TermQuery;

namespace EventsManager.API.Services.Implementations;

public sealed class EventService : IEventService
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<EventService> _logger;

    public EventService(IElasticsearchService elasticsearchService,
        ILogger<EventService> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }
    
    public async Task<BaseResponse<PaginatedResponse<EventResponse>>> GetEvents(EventsFilter filter)
    {
        try
        {
            var searchQuery = new TermQuery
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
            {
                searchQuery = searchQuery && new MatchQuery
                {
                    Field = "location.City",
                    Query = filter.City
                };
            }
            
            if (!string.IsNullOrEmpty(filter.ParticipantName))
            {
                searchQuery = searchQuery && new WildcardQuery
                {
                    Field = "participants.name",
                    Value = $"*{filter.ParticipantName}*"
                };
            }
            
            if (!string.IsNullOrEmpty(filter.Title))
            {
                searchQuery = searchQuery && new MatchQuery
                {
                    Field = "title",
                    Query = filter.Title
                };
            }
            
            if (!string.IsNullOrEmpty(filter.Description))
            {
                searchQuery = searchQuery && new MatchQuery
                {
                    Field = "description",
                    Query = filter.Description
                };
            }
            
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
            {
                return CommonResponses.ErrorResponse.FailedDependencyErrorResponse<PaginatedResponse<EventResponse>>();
            }

            var eventResponseList = searchResponse.Documents.Adapt<List<EventResponse>>();
            var response = new PaginatedResponse<EventResponse>
            {
                Data = eventResponseList,
                PageSize = filter.PageSize,
                CurrentPage = filter.CurrentPage,
                TotalRecords = (int)searchResponse.Total,
                TotalPages = (int) Math.Ceiling((decimal) searchResponse.Total / filter.PageSize)
            };

            return CommonResponses.SuccessResponse.OkResponse(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured getting events by filter\n{filter}",
            JsonConvert.SerializeObject(filter, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<PaginatedResponse<EventResponse>>();
        }
    }

    public async Task<BaseResponse<EventResponse>> GetEventById(string id)
    {
        try
        {
            Event @event = await _elasticsearchService.GetByIdAsync<Event>(id);

            return @event == null
                ? CommonResponses.ErrorResponse.NotFoundErrorResponse<EventResponse>("Event not found")
                : CommonResponses.SuccessResponse.OkResponse(@event.Adapt<EventResponse>());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured getting event by id:{eventId}", id);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<EventResponse>();
        }
    }

    public async Task<BaseResponse<EmptyResponse>> DeleteEventById(string id)
    {
        try
        {
            bool isDeleted = await _elasticsearchService.DeleteAsync<EventResponse>(id);

            return isDeleted
                ? CommonResponses.SuccessResponse.DeletedResponse()
                : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EmptyResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured deleting event by id:{eventId}", id);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<EmptyResponse>();
        }
    }

    public async Task<BaseResponse<EventResponse>> CreateEvent(CreateEventRequest request)
    {
        try
        {
            ValidationResult validateResponse = await new CreateEventRequestValidator().ValidateAsync(request);

            if (!validateResponse.IsValid)
            {
                return CommonResponses.ErrorResponse.BadRequestResponse<EventResponse>("Provide all required inputs");
            }

            Event newEvent = request.Adapt<Event>();
            bool isCreated = await _elasticsearchService.AddAsync(newEvent);

            return isCreated
                ? CommonResponses.SuccessResponse.CreatedResponse(newEvent.Adapt<EventResponse>())
                : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EventResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured creating event\n{event}",
                JsonConvert.SerializeObject(request, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<EventResponse>();
        }
    }

    public async Task<BaseResponse<EventResponse>> AddParticipant(string id,  EventParticipantRequest participant)
    {
        try
        {
            Event @event = await _elasticsearchService.GetByIdAsync<Event>(id);

            if (@event is null)
            {
                return CommonResponses.ErrorResponse.NotFoundErrorResponse<EventResponse>("Event not found");
            }

            bool userAlreadyAdded = @event.Participants.Any(x => x.Username.ToLower().Equals(participant.Username.ToLower()) || 
                                                                 x.Email.ToLower().Equals(participant.Email.ToLower()));

            if (userAlreadyAdded)
            {
                return CommonResponses.ErrorResponse
                    .ConflictErrorResponse<EventResponse>("User already added as participant");
            }
            
            @event.Participants.Add(participant.Adapt<EventParticipant>());
            @event.UpdatedAt = DateTime.UtcNow;

            bool isUpdated = await _elasticsearchService.UpdateAsync(id, @event);

            return isUpdated
                ? CommonResponses.SuccessResponse
                    .OkResponse(@event.Adapt<EventResponse>(), "Participant added successfully")
                : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EventResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{eventId}: An error occured adding participant to event\n{participant}",
                id, JsonConvert.SerializeObject(participant, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<EventResponse>();
        }
    }

    public async Task<BaseResponse<EventResponse>> RemoveParticipant(string id, string username)
    {
        try
        {
            Event @event = await _elasticsearchService.GetByIdAsync<Event>(id);

            if (@event is null)
            {
                return CommonResponses.ErrorResponse.NotFoundErrorResponse<EventResponse>("Event not found");
            }

            EventParticipant eventParticipant = @event.Participants.FirstOrDefault(x => x.Username.Equals(username));

            if (eventParticipant is null)
            {
                return CommonResponses.ErrorResponse
                    .NotFoundErrorResponse<EventResponse>("User is not part of event participants");
            }
            
            @event.Participants.Remove(eventParticipant);
            @event.UpdatedAt = DateTime.UtcNow;

            bool isUpdated = await _elasticsearchService.UpdateAsync(id, @event);

            return isUpdated
                ? CommonResponses.SuccessResponse
                    .OkResponse(@event.Adapt<EventResponse>(), "Participant added successfully")
                : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EventResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured removing event participant:{username} from event with id:{eventId}", 
                username, id);

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<EventResponse>();
        }
    }
}