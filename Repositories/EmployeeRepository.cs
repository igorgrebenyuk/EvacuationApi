using Microsoft.EntityFrameworkCore;
using EvacuationApi.Data;
using EvacuationApi.Models;

namespace EvacuationApi.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Employee>> GetAllAsync(int? shift = null, bool subjectOnly = false)
    {
        var query = _db.Employees.AsQueryable();
        if (shift is not null)
            query = query.Where(e => e.Shift == shift);
        if (subjectOnly)
            query = query.Where(e => e.IsSubjectToEvacuation);

        return await query.OrderBy(e => e.Shift).ThenBy(e => e.FullName).ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id) =>
        await _db.Employees.FindAsync(id);

    public async Task<int> CountInVehicleAsync(int vehicleId, int exceptEmployeeId = 0) =>
        await _db.Employees.CountAsync(e => e.VehicleId == vehicleId && e.Id != exceptEmployeeId);

    public async Task<int> CountInPointAsync(int pointId, int exceptEmployeeId = 0) =>
        await _db.Employees.CountAsync(e => e.ReceptionPointId == pointId && e.Id != exceptEmployeeId);

    public async Task AddAsync(Employee employee) =>
        await _db.Employees.AddAsync(employee);

    public void Remove(Employee employee) =>
        _db.Employees.Remove(employee);

    public async Task<bool> SaveChangesAsync() =>
        await _db.SaveChangesAsync() > 0;
}
