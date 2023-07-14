using System.Net.Mime;
using EventsManager.API.Models.Filters;
using EventsManager.API.Models.Requests;
using EventsManager.API.Models.Responses;
using EventsManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsManager.API.Controllers;

[ApiController]
[Route("api/events/invitations")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse<EmptyResponse>))]
[ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(BaseResponse<EmptyResponse>))]
public class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;

    public InvitationsController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    /// <summary>
    /// Create event invitation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BaseResponse<EventInvitationResponse>))]
    public async Task<IActionResult> CreateEventInvitation([FromBody] EventInvitationRequest request)
    {
        var response = await _invitationService.CreateInvitation(request);
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Get invitation by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:required}")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<EventInvitationResponse>))]
    public async Task<IActionResult> GetInvitationById([FromRoute] string id)
    {
        var response = await _invitationService.GetInvitationById(id);
        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Get all invitations with filter
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<PaginatedResponse<EventInvitationResponse>>))]
    public async Task<IActionResult> GetEventInvitations([FromQuery] EventInvitationsFilter filter)
    {
        var response = await _invitationService.GetEventInvitations(filter);
        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Decline invitation by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:required}/decline")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<EventInvitationResponse>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(BaseResponse<EmptyResponse>))]
    public async Task<IActionResult> DeclineInvitation([FromRoute] string id)
    {
        var response = await _invitationService.DeclineInvitation(id);
        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Accept invitation by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:required}/accept")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<EventInvitationResponse>))]
    public async Task<IActionResult> AcceptInvitation([FromRoute] string id)
    {
        var response = await _invitationService.AcceptInvitation(id);
        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Delete invitation by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:required}")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<EmptyResponse>))]
    public async Task<IActionResult> DeleteInvitation([FromRoute] string id)
    {
        var response = await _invitationService.DeleteInvitation(id);
        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Get user invitations that are pending approval
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username:required}/approvals/pending")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<List<EventInvitationResponse>>))]
    public async Task<IActionResult> GetUserInvitationsPendingApproval([FromRoute] string username)
    {
        var response = await _invitationService.GetUserInvitationsPendingApproval(username);
        return StatusCode(response.Code, response);
    }
}