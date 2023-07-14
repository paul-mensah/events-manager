using Arch.EntityFrameworkCore.UnitOfWork.Collections;
using EventsManager.API.Models.Filters;
using EventsManager.API.Models.Responses;
using EventsManager.API.Storage.Data;
using EventsManager.API.Storage.Domain.EventInvitations;
using EventsManager.API.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.API.Storage.Repositories.Implementations;

public class EventInvitationRepository : RepositoryBase<EventInvitation>, IEventInvitationRepository
{
    private readonly ApplicationDatabaseContext _dbContext;

    public EventInvitationRepository(ApplicationDatabaseContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<EventInvitation>> GetEventInvitations(EventInvitationsFilter filter)
    {
        var eventInvitationQueryable = _dbContext.EventInvitations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(filter.Username))
        {
            eventInvitationQueryable = eventInvitationQueryable.Where(x => x.Username.Equals(filter.Username));
        }
        
        if (!string.IsNullOrEmpty(filter.Email))
        {
            eventInvitationQueryable = eventInvitationQueryable.Where(x => x.Email.ToLower().Equals(filter.Email.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(filter.Name))
        {
            eventInvitationQueryable = eventInvitationQueryable.Where(x => x.Name.ToLower().Equals(filter.Name.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(filter.EventId))
        {
            eventInvitationQueryable = eventInvitationQueryable.Where(x => x.EventId.Equals(filter.EventId));
        }
        
        if (!string.IsNullOrEmpty(filter.Title))
        {
            eventInvitationQueryable = eventInvitationQueryable.Where(x => x.Title.ToLower().Contains(filter.Title.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(filter.InvitedBy))
        {
            eventInvitationQueryable = eventInvitationQueryable.Where(x => x.InvitedBy.Equals(filter.InvitedBy));
        }
        
        if (filter.IsAccepted.HasValue)
        {
            eventInvitationQueryable = eventInvitationQueryable.Where(x => x.IsAccepted.Equals(filter.IsAccepted.Value));
        }
        
        if (filter.AcceptedDate.HasValue)
        {
            eventInvitationQueryable = eventInvitationQueryable.Where(x => x.AcceptedDate.Equals(filter.AcceptedDate.Value));
        }

        eventInvitationQueryable = "desc".Equals(filter.OrderBy, StringComparison.OrdinalIgnoreCase)
            ? eventInvitationQueryable.OrderByDescending(x => x.CreatedAt)
            : eventInvitationQueryable.OrderBy(x => x.CreatedAt);

        var paginatedResponse = await eventInvitationQueryable.ToPagedListAsync(filter.CurrentPage - 1, filter.PageSize);

        return new PaginatedResponse<EventInvitation>
        {
            CurrentPage = filter.CurrentPage,
            PageSize = filter.PageSize,
            Data = paginatedResponse.Items.ToList(),
            TotalPages = paginatedResponse.TotalPages,
            TotalRecords = paginatedResponse.TotalCount
        };
    }

    public async Task<List<EventInvitation>> GetUserInvitationsPendingApproval(string username)
    {
        return await _dbContext.EventInvitations
            .AsNoTracking()
            .Where(x => x.Username.Equals(username) && !x.IsAccepted)
            .ToListAsync();
    }
}