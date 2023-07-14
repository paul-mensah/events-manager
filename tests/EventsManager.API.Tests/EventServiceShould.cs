using EventsManager.API.Models.Responses;
using EventsManager.API.Services.Implementations;
using EventsManager.API.Services.Interfaces;
using EventsManager.API.Storage.Domain.Events;
using EventsManager.Api.Tests.Setup;
using FluentAssertions;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EventsManager.Api.Tests;

public class EventServiceShould : IClassFixture<TestFixture>
{
    private readonly Mock<IElasticsearchService> _elasticSearchServiceMock;
    private readonly EventService _eventService;

    public EventServiceShould(TestFixture fixture)
    {
        var logger = fixture.ServiceProvider.GetRequiredService<ILogger<EventService>>();
        _elasticSearchServiceMock = new Mock<IElasticsearchService>();
        _eventService = new EventService(_elasticSearchServiceMock.Object, logger);
    }

    [Fact]
    public async Task Return_200_OK_Status_And_An_Event_When_An_Id_Of_An_Existing_Event_Is_Provided()
    {
        // Arrange
        Event testEvent = EventFactory.GenerateEvent();
        var mockResponse = CommonResponses.SuccessResponse.OkResponse(testEvent.Adapt<EventResponse>());

        _elasticSearchServiceMock.Setup(x => x.GetByIdAsync<Event>(It.IsAny<string>()))
            .ReturnsAsync(testEvent);

        // Act
        BaseResponse<EventResponse> response = await _eventService.GetEventById(testEvent.Id);

        // Assert
        response.Code.Should().Be(mockResponse.Code);
        response.Message.Should().Be(mockResponse.Message);
        response.Data.Should().NotBeNull();
        response.Data.Id.Should().Be(testEvent.Id);
        response.Data.Participants.Count.Should().Be(testEvent.Participants.Count);
    }
    
    [Fact]
    public async Task Return_400_BadRequest_Status_When_An_Id_Of_An_Event_Which_Does_Not_Exist_Is_Provided()
    {
        // Arrange
        var mockResponse = CommonResponses.ErrorResponse.NotFoundErrorResponse<EventResponse>("Event not found");

        _elasticSearchServiceMock.Setup(x => x.GetByIdAsync<Event>(It.IsAny<string>()))
            .ReturnsAsync( () => null!);

        // Act
        BaseResponse<EventResponse> response = await _eventService.GetEventById(Guid.NewGuid().ToString("N"));

        // Assert
        response.Code.Should().Be(mockResponse.Code);
        response.Message.Should().Be(mockResponse.Message);
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task Return_200_OK_Status_When_An_Existing_Event_Is_Deleted()
    {
        // Arrange
        var mockResponse = CommonResponses.SuccessResponse.DeletedResponse();

        _elasticSearchServiceMock.Setup(x => x.DeleteAsync<EventResponse>(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        BaseResponse<EmptyResponse> response = await _eventService.DeleteEventById(Guid.NewGuid().ToString("N"));

        // Assert
        response.Code.Should().Be(mockResponse.Code);
        response.Message.Should().Be(mockResponse.Message);
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task Return_424_FailedDependency_Status_When_Deleting_A_Non_Existing_Event()
    {
        // Arrange
        var mockResponse = CommonResponses.ErrorResponse.FailedDependencyErrorResponse<EmptyResponse>();

        _elasticSearchServiceMock.Setup(x => x.DeleteAsync<EventResponse>(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        BaseResponse<EmptyResponse> response = await _eventService.DeleteEventById(Guid.NewGuid().ToString("N"));

        // Assert
        response.Code.Should().Be(mockResponse.Code);
        response.Message.Should().Be(mockResponse.Message);
        response.Data.Should().BeNull();
    }
}