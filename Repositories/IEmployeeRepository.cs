using EvacuationApi.Models;

namespace EvacuationApi.Repositories;

public interface IEmployeeRepository
{
    /// <summary>
    /// Сотрудники (отслеживаются контекстом). Фильтры: смена, только подлежащие эвакуации
    /// </summary>
    Task<List<Employee>> GetAllAsync(int? shift = null, bool subjectOnly = false);
    Task<Employee?> GetByIdAsync(int id);

    /// <summary>
    /// Сколько сотрудников назначено в транспорт (кроме указанного сотрудника)
    /// </summary>
    Task<int> CountInVehicleAsync(int vehicleId, int exceptEmployeeId = 0);

    /// <summary>
    /// Сколько сотрудников назначено в пункт размещения (кроме указанного сотрудника)
    /// </summary>
    Task<int> CountInPointAsync(int pointId, int exceptEmployeeId = 0);

    Task AddAsync(Employee employee);
    void Remove(Employee employee);
    Task<bool> SaveChangesAsync();
}
