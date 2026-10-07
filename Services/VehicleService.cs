using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Repositories;

namespace EvacuationApi.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _repository;
    private readonly IEmployeeRepository _employees;

    public VehicleService(IVehicleRepository repository, IEmployeeRepository employees)
    {
        _repository = repository;
        _employees = employees;
    }

    public Task<List<Vehicle>> GetAllAsync(VehicleOwnership? ownership = null) =>
        _repository.GetAllAsync(ownership);

    public Task<Vehicle?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task<Vehicle> CreateAsync(VehicleUpsertDto dto)
    {
        Validate(dto);
        var vehicle = MapToEntity(new Vehicle(), dto);
        await _repository.AddAsync(vehicle);
        await _repository.SaveChangesAsync();
        return vehicle;
    }

    public async Task<bool> UpdateAsync(int id, VehicleUpsertDto dto)
    {
        var vehicle = await _repository.GetByIdAsync(id);
        if (vehicle is null) return false;

        Validate(dto);

        var assigned = await _employees.CountInVehicleAsync(id);
        if (dto.Capacity < assigned)
            throw new InvalidOperationException(
                $"Нельзя уменьшить вместимость до {dto.Capacity}: в транспорт уже назначено {assigned} чел.");

        MapToEntity(vehicle, dto);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vehicle = await _repository.GetByIdAsync(id);
        if (vehicle is null) return false;

        // Назначения сотрудников обнуляются на уровне БД (ON DELETE SET NULL).
        _repository.Remove(vehicle);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static void Validate(VehicleUpsertDto dto)
    {
        if (dto.Ownership == VehicleOwnership.Contracted && string.IsNullOrWhiteSpace(dto.ProviderName))
            throw new InvalidOperationException(
                "Для привлечённого транспорта укажите организацию-поставщика (договор).");
    }

    private static Vehicle MapToEntity(Vehicle vehicle, VehicleUpsertDto dto)
    {
        vehicle.Name = dto.Name;
        vehicle.PlateNumber = dto.PlateNumber;
        vehicle.Type = dto.Type;
        vehicle.Capacity = dto.Capacity;
        vehicle.Ownership = dto.Ownership;
        vehicle.ProviderName = dto.ProviderName;
        vehicle.ContractNumber = dto.ContractNumber;
        vehicle.ContractValidUntil = dto.ContractValidUntil;
        vehicle.DriverName = dto.DriverName;
        vehicle.DriverPhone = dto.DriverPhone;
        vehicle.Status = dto.Status;
        return vehicle;
    }
}
