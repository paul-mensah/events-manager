using EventsManager.API.Extensions;
using EventsManager.API.Services.Interfaces;
using EventsManager.API.Storage.Domain.Events;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EventsManager.API.Services.Implementations;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<bool> AddToHashSetAsync<T>(T value, string key, string hashField)
    {
        return await _connectionMultiplexer.GetDatabase()
            .HashSetAsync(key, hashField, JsonConvert.SerializeObject(value));
    }

    public async Task<bool> DeleteFromHashSetAsync(string key, string hashField)
    {
        return await _connectionMultiplexer.GetDatabase().HashDeleteAsync(key, hashField);
    }

    public async Task<bool> HashExistsAsync(string key, string hashField)
    {
        return await _connectionMultiplexer.GetDatabase()
            .HashExistsAsync(key, hashField);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(string key)
    {
        var invitationsRedisValue = await _connectionMultiplexer.GetDatabase().HashGetAllAsync(key);

        return invitationsRedisValue
            .Select(x => JsonConvert.DeserializeObject<T>(x.Value));
    }

    public async Task<IEnumerable<CachedEventInvitation>> GetUserEventInvitationsByUsername(string username)
    {
        string invitationsKey = RedisConstants.GetUserInvitationsRedisKeyByUsername(username);
        var invitationsRedisValue = await _connectionMultiplexer.GetDatabase().HashGetAllAsync(invitationsKey);

        return invitationsRedisValue
            .Select(x => JsonConvert.DeserializeObject<CachedEventInvitation>(x.Value));
    }
}