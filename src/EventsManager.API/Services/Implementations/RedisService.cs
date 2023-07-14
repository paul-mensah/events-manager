using EventsManager.API.Extensions;
using EventsManager.API.Services.Interfaces;
using EventsManager.API.Storage.Domain.Events;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace EventsManager.API.Services.Implementations;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<IEnumerable<CachedEventInvitation>> GetUserEventInvitationsByUsername(string username)
    {
        try
        {
            string invitationsKey = RedisConstants.GetUserInvitationsRedisKeyByUsername(username);
            var invitationsRedisValue = await _connectionMultiplexer.GetDatabase().HashGetAllAsync(invitationsKey);

            return invitationsRedisValue
                .Select(x => JsonConvert.DeserializeObject<CachedEventInvitation>(x.Value));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured getting user event invitations by username:{username}", username);
            return null;
        }
    }

    public async Task<bool> AddToHashSetAsync<T>(T value, string key, string hashField)
    {
        try
        {
            return await _connectionMultiplexer.GetDatabase()
                .HashSetAsync(
                    key: key, 
                    hashField: hashField,
                    value: JsonConvert.SerializeObject(value));
        }
        catch (Exception e)
        {
            _logger.LogError(e, 
                "An error occured adding value to hash set with key:{key} and field:{field}\nValue:{value}",
                key, hashField, JsonConvert.SerializeObject(value, Formatting.Indented));

            return false;
        }
    }

    public async Task<bool> DeleteFromHashSetAsync(string key, string hashField)
    {
        try
        {
            return await _connectionMultiplexer.GetDatabase()
                .HashDeleteAsync(
                    key: key,
                    hashField: hashField);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured deleting value from hash set");
            return false;
        }
    }

    public async Task<bool> HashExistsAsync(string key, string hashField)
    {
        try
        {
            return await _connectionMultiplexer.GetDatabase()
                .HashExistsAsync(key, hashField);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured checking if hashSet:{key} contains hashField:{hashField}",
                key, hashField);

            return false;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(string key)
    {
        try
        {
            var invitationsRedisValue = await _connectionMultiplexer.GetDatabase().HashGetAllAsync(key);

            return invitationsRedisValue
                .Select(x => JsonConvert.DeserializeObject<T>(x.Value));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured getting all values in hash set:{key}", key);
            return Array.Empty<T>();
        }
    }
}