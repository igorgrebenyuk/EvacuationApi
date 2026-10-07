using Microsoft.EntityFrameworkCore;
using EvacuationApi.Data;
using EvacuationApi.Models;

namespace EvacuationApi.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _db;

    public VehicleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Vehicle>> GetAllAsync(VehicleOwnership? ownership = null)
    {
        var query = _db.Vehicles.AsQueryable();
        if (ownership is not null)
            query = query.Where(v => v.Ownership == ownership);

        return await query.OrderBy(v => v.Ownership).ThenBy(v => v.Id).ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id) =>
        await _db.Vehicles.FindAsync(id);

    public async Task AddAsync(Vehicle vehicle) =>
        await _db.Vehicles.AddAsync(vehicle);

    public void Remove(Vehicle vehicle) =>
        _db.Vehicles.Remove(vehicle);

    public async Task<bool> SaveChangesAsync() =>
        await _db.SaveChangesAsync() > 0;
}
