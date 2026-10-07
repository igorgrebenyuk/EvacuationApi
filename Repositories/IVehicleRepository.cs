using EvacuationApi.Models;

namespace EvacuationApi.Repositories;

public interface IVehicleRepository
{
    Task<List<Vehicle>> GetAllAsync(VehicleOwnership? ownership = null);
    Task<Vehicle?> GetByIdAsync(int id);
    Task AddAsync(Vehicle vehicle);
    void Remove(Vehicle vehicle);
    Task<bool> SaveChangesAsync();
}
