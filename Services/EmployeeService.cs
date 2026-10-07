using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Repositories;

namespace EvacuationApi.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IVehicleRepository _vehicles;
    private readonly IReceptionPointRepository _points;

    public EmployeeService(
        IEmployeeRepository repository,
        IVehicleRepository vehicles,
        IReceptionPointRepository points)
    {
        _repository = repository;
        _vehicles = vehicles;
        _points = points;
    }

    public Task<List<Employee>> GetAllAsync(int? shift = null, bool subjectOnly = false) =>
        _repository.GetAllAsync(shift, subjectOnly);

    public Task<Employee?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task<Employee> CreateAsync(EmployeeUpsertDto dto)
    {
        var employee = MapToEntity(new Employee(), dto);
        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();
        return employee;
    }

    public async Task<bool> UpdateAsync(int id, EmployeeUpsertDto dto)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee is null) return false;

        MapToEntity(employee, dto);

        // Сотрудник, остающийся на объекте, в эвакуации не участвует — назначения снимаются.
        if (!employee.IsSubjectToEvacuation)
        {
            employee.VehicleId = null;
            employee.ReceptionPointId = null;
        }

        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignAsync(int id, EmployeeAssignmentDto dto)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee is null) return false;

        var hasAssignment = dto.VehicleId is not null || dto.ReceptionPointId is not null;
        if (hasAssignment && !employee.IsSubjectToEvacuation)
            throw new InvalidOperationException("Сотрудник не подлежит эвакуации — назначать ему транспорт и размещение нельзя.");

        if (dto.VehicleId is int vehicleId)
        {
            var vehicle = await _vehicles.GetByIdAsync(vehicleId)
                ?? throw new InvalidOperationException("Транспортное средство не найдено.");

            if (!vehicle.IsAvailableForEvacuation)
                throw new InvalidOperationException("Транспорт недоступен: неисправен или истёк договор.");

            var used = await _repository.CountInVehicleAsync(vehicleId, id);
            if (used >= vehicle.Capacity)
                throw new InvalidOperationException($"В транспорте «{vehicle.Name}» нет свободных мест ({vehicle.Capacity}).");
        }

        if (dto.ReceptionPointId is int pointId)
        {
            var point = await _points.GetByIdAsync(pointId)
                ?? throw new InvalidOperationException("Пункт размещения не найден.");

            var used = await _repository.CountInPointAsync(pointId, id);
            if (used >= point.Capacity)
                throw new InvalidOperationException($"В пункте «{point.Name}» нет свободных мест ({point.Capacity}).");
        }

        employee.VehicleId = dto.VehicleId;
        employee.ReceptionPointId = dto.ReceptionPointId;
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee is null) return false;

        _repository.Remove(employee);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static Employee MapToEntity(Employee employee, EmployeeUpsertDto dto)
    {
        employee.FullName = dto.FullName;
        employee.Position = dto.Position;
        employee.Department = dto.Department;
        employee.Shift = dto.Shift;
        employee.IsSubjectToEvacuation = dto.IsSubjectToEvacuation;
        employee.NeedsAssistance = dto.NeedsAssistance;
        employee.Phone = dto.Phone;
        return employee;
    }
}
