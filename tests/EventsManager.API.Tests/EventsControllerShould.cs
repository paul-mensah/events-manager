using EventsManager.API.Controllers;
using EventsManager.API.Models.Requests;
using EventsManager.API.Models.Responses;
using EventsManager.API.Services.Interfaces;
using EventsManager.API.Storage.Domain.Events;
using EventsManager.Api.Tests.Setup;
using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EventsManager.Api.Tests;

public class EventsControllerShould : IClassFixture<TestFixture>
{
    private readonly Mock<IEventService> _eventServiceMock;
    private readonly EventsController _eventsController;
    
    public EventsControllerShould()
    {
        _eventServiceMock = new Mock<IEventService>();
        _eventsController = new EventsController(_eventServiceMock.Object);
    }

    [Fact]
    public async Task Return_200OK_Response_When_An_Id_Of_An_Existing_Event_Is_Provided()
    {
        // Arrange
        Event testEvent = EventFactory.GenerateEvent();
        var mockResponse = CommonResponses.SuccessResponse
            .OkResponse(testEvent.Adapt<EventResponse>());
        
        _eventServiceMock.Setup(x => x.GetEventById(It.IsAny<string>()))
            .ReturnsAsync(mockResponse);
        
        // Act
        ObjectResult? response = (ObjectResult)await _eventsController.GetEventById(testEvent.Id);
        var parsedResponse = response.Value as BaseResponse<EventResponse>;

        // Assert
        response.StatusCode.Should().Be(mockResponse.Code);
        parsedResponse?.Code.Should().Be(mockResponse.Code);
        parsedResponse?.Message.Should().Be(mockResponse.Message);
        parsedResponse?.Data.Id.Should().Be(testEvent.Id);
    }
    
    [Fact]
    public async Task Return_404NotFound_Response_When_An_Id_Of_An_Event_Which_Does_Not_Exist_Is_Provided()
    {
        // Arrange
        var mockResponse = CommonResponses.ErrorResponse
            .NotFoundErrorResponse<EventResponse>("Event not found");
        
        _eventServiceMock.Setup(x => x.GetEventById(It.IsAny<string>()))
            .ReturnsAsync(mockResponse);
        
        // Act
        ObjectResult? response = (ObjectResult)await _eventsController.GetEventById(Guid.NewGuid().ToString("N"));
        var parsedResponse = response.Value as BaseResponse<EventResponse>;

        // Assert
        response.StatusCode.Should().Be(mockResponse.Code);
        parsedResponse?.Code.Should().Be(mockResponse.Code);
        parsedResponse?.Message.Should().Be(mockResponse.Message);
        parsedResponse?.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task Return_200OK_Response_When_An_Existing_Event_Is_Deleted()
    {
        // Arrange
        var mockResponse = CommonResponses.SuccessResponse
            .DeletedResponse();
        
        _eventServiceMock.Setup(x => x.DeleteEventById(It.IsAny<string>()))
            .ReturnsAsync(mockResponse);
        
        // Act
        ObjectResult? response = (ObjectResult)await _eventsController.DeleteEventById(Guid.NewGuid().ToString("N"));
        var parsedResponse = response.Value as BaseResponse<EventResponse>;

        // Assert
        response.StatusCode.Should().Be(mockResponse.Code);
        parsedResponse?.Code.Should().Be(mockResponse.Code);
        parsedResponse?.Message.Should().Be(mockResponse.Message);
        parsedResponse?.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task Return_424FailedDependency_Response_When_An_Error_Occurs_Deleting_An_Event()
    {
        // Arrange
        var mockResponse = CommonResponses.ErrorResponse
            .FailedDependencyErrorResponse<EmptyResponse>();
        
        _eventServiceMock.Setup(x => x.DeleteEventById(It.IsAny<string>()))
            .ReturnsAsync(mockResponse);
        
        // Act
        ObjectResult? response = (ObjectResult)await _eventsController.DeleteEventById(Guid.NewGuid().ToString("N"));
        var parsedResponse = response.Value as BaseResponse<EventResponse>;

        // Assert
        response.StatusCode.Should().Be(mockResponse.Code);
        parsedResponse?.Code.Should().Be(mockResponse.Code);
        parsedResponse?.Message.Should().Be(mockResponse.Message);
        parsedResponse?.Data.Should().BeNull();
    }

    [Fact]
    public async Task Return_201Created_Response_When_Correct_Event_Details_Are_Provided()
    {
        // Arrange
        CreateEventRequest eventRequest = EventFactory.GenerateEvent().Adapt<CreateEventRequest>();
        var mockResponse = CommonResponses.SuccessResponse
            .CreatedResponse(eventRequest.Adapt<EventResponse>());
        
        _eventServiceMock.Setup(x => x.CreateEvent(It.IsAny<CreateEventRequest>()))
            .ReturnsAsync(mockResponse);
        
        // Act
        ObjectResult? response = (ObjectResult)await _eventsController.CreateEvent(eventRequest);
        var parsedResponse = response.Value as BaseResponse<EventResponse>;

        // Assert
        response.StatusCode.Should().Be(mockResponse.Code);
        parsedResponse?.Code.Should().Be(mockResponse.Code);
        parsedResponse?.Message.Should().Be(mockResponse.Message);
        parsedResponse?.Data.Should().NotBeNull();

        parsedResponse?.Data.Title.Should().Be(eventRequest.Title);
        parsedResponse?.Data.Description.Should().Be(eventRequest.Description);
        parsedResponse?.Data.PhotoUrl.Should().Be(eventRequest.PhotoUrl);
        parsedResponse?.Data.Location.City.Should().Be(eventRequest.Location.City);
        parsedResponse?.Data.Location.ZipCode.Should().Be(eventRequest.Location.ZipCode);
        parsedResponse?.Data.StartDate.Should().Be(eventRequest.StartDate);
        parsedResponse?.Data.EndDate.Should().Be(eventRequest.EndDate);
        parsedResponse?.Data.CreatedBy.Should().Be(eventRequest.CreatedBy);
    }

    [Fact]
    public async Task Return_400BadRequest_Response_When_InCorrect_Event_Details_Are_Provided()
    {
        // Arrange
        CreateEventRequest eventRequest = new CreateEventRequest();

        var mockResponse = CommonResponses.ErrorResponse
            .BadRequestResponse<EventResponse>(string.Empty);
        
        _eventServiceMock.Setup(x => x.CreateEvent(It.IsAny<CreateEventRequest>()))
            .ReturnsAsync(mockResponse);
        
        // Act
        ObjectResult? response = (ObjectResult)await _eventsController.CreateEvent(eventRequest);
        var parsedResponse = response.Value as BaseResponse<EventResponse>;

        // Assert
        response.StatusCode.Should().Be(mockResponse.Code);
        parsedResponse?.Code.Should().Be(mockResponse.Code);
        parsedResponse?.Data.Should().BeNull();
    }
}