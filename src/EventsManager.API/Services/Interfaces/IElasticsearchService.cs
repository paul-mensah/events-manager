using Nest;

namespace EventsManager.API.Services.Interfaces;

public interface IElasticsearchService
{
    Task<T> GetByIdAsync<T>(string id) where T : class;
    Task<bool> AddAsync<T>(T doc) where T : class;
    Task<bool> DeleteAsync<T>(string id) where T : class;
    Task<bool> UpdateAsync<T>(string id, T doc) where T : class;
    Task<ISearchResponse<T>> SearchAsync<T>(SearchRequest<T> searchRequest) where T : class;
}