using EvacuationApi.DTOs;
using EvacuationApi.Models;

namespace EvacuationApi.Services;

public interface IEmployeeService
{
    Task<List<Employee>> GetAllAsync(int? shift = null, bool subjectOnly = false);
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(EmployeeUpsertDto dto);
    Task<bool> UpdateAsync(int id, EmployeeUpsertDto dto);

    /// <summary>
    /// Ручное назначение транспорта / пункта размещения. false — сотрудник не найден.
    /// Бросает InvalidOperationException, если транспорт/пункт не найден, недоступен или заполнен.
    /// </summary>
    Task<bool> AssignAsync(int id, EmployeeAssignmentDto dto);
    Task<bool> DeleteAsync(int id);
}
