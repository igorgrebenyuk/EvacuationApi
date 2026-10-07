using Microsoft.EntityFrameworkCore;
using EvacuationApi.Models;

namespace EvacuationApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<EvacuationRoute> Routes => Set<EvacuationRoute>();
    public DbSet<ReceptionPoint> ReceptionPoints => Set<ReceptionPoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Связи без навигационных свойств (в JSON нет циклов).
        // При удалении транспорта / пункта назначения у сотрудников и маршрутов просто обнуляются.
        modelBuilder.Entity<Employee>()
            .HasOne<Vehicle>().WithMany()
            .HasForeignKey(e => e.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Employee>()
            .HasOne<ReceptionPoint>().WithMany()
            .HasForeignKey(e => e.ReceptionPointId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EvacuationRoute>()
            .HasOne<ReceptionPoint>().WithMany()
            .HasForeignKey(r => r.ReceptionPointId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Employee>().HasIndex(e => e.Shift);
    }
}
