using EvacuationApi.DTOs;
using EvacuationApi.Models;

namespace EvacuationApi.Services;

public interface IVehicleService
{
    Task<List<Vehicle>> GetAllAsync(VehicleOwnership? ownership = null);
    Task<Vehicle?> GetByIdAsync(int id);

    /// <summary>
    /// Бросает InvalidOperationException при нарушении бизнес-правил
    /// </summary>
    Task<Vehicle> CreateAsync(VehicleUpsertDto dto);
    Task<bool> UpdateAsync(int id, VehicleUpsertDto dto);
    Task<bool> DeleteAsync(int id);
}
