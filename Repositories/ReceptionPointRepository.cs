using Microsoft.EntityFrameworkCore;
using EvacuationApi.Data;
using EvacuationApi.Models;

namespace EvacuationApi.Repositories;

public class ReceptionPointRepository : IReceptionPointRepository
{
    private readonly AppDbContext _db;

    public ReceptionPointRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ReceptionPoint>> GetAllAsync() =>
        await _db.ReceptionPoints.OrderBy(p => p.Id).ToListAsync();

    public async Task<ReceptionPoint?> GetByIdAsync(int id) =>
        await _db.ReceptionPoints.FindAsync(id);

    public async Task AddAsync(ReceptionPoint point) =>
        await _db.ReceptionPoints.AddAsync(point);

    public void Remove(ReceptionPoint point) =>
        _db.ReceptionPoints.Remove(point);

    public async Task<bool> SaveChangesAsync() =>
        await _db.SaveChangesAsync() > 0;
}
