using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Repositories;

namespace EvacuationApi.Services;

public class ReceptionPointService : IReceptionPointService
{
    private readonly IReceptionPointRepository _repository;
    private readonly IEmployeeRepository _employees;

    public ReceptionPointService(IReceptionPointRepository repository, IEmployeeRepository employees)
    {
        _repository = repository;
        _employees = employees;
    }

    public Task<List<ReceptionPoint>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<ReceptionPoint?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task<ReceptionPoint> CreateAsync(ReceptionPointUpsertDto dto)
    {
        var point = MapToEntity(new ReceptionPoint(), dto);
        await _repository.AddAsync(point);
        await _repository.SaveChangesAsync();
        return point;
    }

    public async Task<bool> UpdateAsync(int id, ReceptionPointUpsertDto dto)
    {
        var point = await _repository.GetByIdAsync(id);
        if (point is null) return false;

        var placed = await _employees.CountInPointAsync(id);
        if (dto.Capacity < placed)
            throw new InvalidOperationException(
                $"Нельзя уменьшить вместимость до {dto.Capacity}: в пункт уже назначено {placed} чел.");

        MapToEntity(point, dto);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var point = await _repository.GetByIdAsync(id);
        if (point is null) return false;

        // Назначения сотрудников и привязки маршрутов обнуляются на уровне БД (ON DELETE SET NULL).
        _repository.Remove(point);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static ReceptionPoint MapToEntity(ReceptionPoint point, ReceptionPointUpsertDto dto)
    {
        point.Name = dto.Name;
        point.Address = dto.Address;
        point.HostOrganization = dto.HostOrganization;
        point.ContactPerson = dto.ContactPerson;
        point.ContactPhone = dto.ContactPhone;
        point.Capacity = dto.Capacity;
        point.AgreementNumber = dto.AgreementNumber;
        point.Notes = dto.Notes;
        return point;
    }
}
