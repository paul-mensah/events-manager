using Bogus;
using EventsManager.API.Storage.Domain.Events;

namespace EventsManager.Api.Tests;

public static class EventFactory
{
    private static readonly Faker Faker = new();

    public static Event GenerateEvent()
    {
        return new Event
        {
            Description = Faker.Random.Words(),
            Id = Guid.NewGuid().ToString("N"),
            Location = new EventLocation
            {
                City = Faker.Address.City(),
                ZipCode = Faker.Address.ZipCode()
            },
            Title = Faker.Random.Words(3),
            CreatedAt = Faker.Date.Recent(),
            CreatedBy = Faker.Person.UserName,
            EndDate = Faker.Date.Future(),
            StartDate = Faker.Date.Past(),
            PhotoUrl = Faker.Person.Avatar,
            Participants = GenerateEventParticipants(new Random().Next(1, 10))
        };
    }

    public static List<EventParticipant> GenerateEventParticipants(int count)
    {
        var participantsList = new List<EventParticipant>();

        for (int i = 0; i < count; i++)
            participantsList.Add(new EventParticipant
            {
                PhotoUrl = Faker.Person.Avatar,
                Email = Faker.Person.Email,
                Name = Faker.Person.FullName,
                Username = Faker.Person.UserName
            });

        return participantsList;
    }
}