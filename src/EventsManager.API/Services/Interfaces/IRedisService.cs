namespace EventsManager.API.Services.Interfaces;

public interface IRedisService
{
    Task<bool> AddToHashSetAsync<T>(T value, string key, string hashField);
    Task<bool> DeleteFromHashSetAsync(string key, string hashField);
    Task<bool> HashExistsAsync(string key, string hashField);
    Task<IEnumerable<T>> GetAllAsync<T>(string key);
}