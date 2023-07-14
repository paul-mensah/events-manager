using EventsManager.API.Storage.Domain;

namespace EventsManager.API.Storage.Repositories.Interfaces;

public interface IRepositoryBase<T> where T : EntityBase
{
    Task<T> GetById(string id);
    Task<bool> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(T entity);
}