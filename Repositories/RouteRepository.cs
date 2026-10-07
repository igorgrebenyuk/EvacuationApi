using Microsoft.EntityFrameworkCore;
using EvacuationApi.Data;
using EvacuationApi.Models;

namespace EvacuationApi.Repositories;

public class RouteRepository : IRouteRepository
{
    private readonly AppDbContext _db;

    public RouteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<EvacuationRoute>> GetAllAsync() =>
        await _db.Routes.OrderBy(r => r.Kind).ThenBy(r => r.Id).ToListAsync();

    public async Task<EvacuationRoute?> GetByIdAsync(int id) =>
        await _db.Routes.FindAsync(id);

    public async Task AddAsync(EvacuationRoute route) =>
        await _db.Routes.AddAsync(route);

    public void Remove(EvacuationRoute route) =>
        _db.Routes.Remove(route);

    public async Task<bool> SaveChangesAsync() =>
        await _db.SaveChangesAsync() > 0;
}
