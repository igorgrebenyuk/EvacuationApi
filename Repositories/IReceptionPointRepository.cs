using EvacuationApi.Models;

namespace EvacuationApi.Repositories;

public interface IReceptionPointRepository
{
    Task<List<ReceptionPoint>> GetAllAsync();
    Task<ReceptionPoint?> GetByIdAsync(int id);
    Task AddAsync(ReceptionPoint point);
    void Remove(ReceptionPoint point);
    Task<bool> SaveChangesAsync();
}
