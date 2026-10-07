using EvacuationApi.Models;

namespace EvacuationApi.Repositories;

public interface IRouteRepository
{
    Task<List<EvacuationRoute>> GetAllAsync();
    Task<EvacuationRoute?> GetByIdAsync(int id);
    Task AddAsync(EvacuationRoute route);
    void Remove(EvacuationRoute route);
    Task<bool> SaveChangesAsync();
}
