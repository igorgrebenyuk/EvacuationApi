using EvacuationApi.DTOs;
using EvacuationApi.Models;

namespace EvacuationApi.Services;

public interface IReceptionPointService
{
    Task<List<ReceptionPoint>> GetAllAsync();
    Task<ReceptionPoint?> GetByIdAsync(int id);
    Task<ReceptionPoint> CreateAsync(ReceptionPointUpsertDto dto);

    /// <summary>
    /// Бросает InvalidOperationException, если вместимость меньше уже размещённых
    /// </summary>
    Task<bool> UpdateAsync(int id, ReceptionPointUpsertDto dto);
    Task<bool> DeleteAsync(int id);
}
