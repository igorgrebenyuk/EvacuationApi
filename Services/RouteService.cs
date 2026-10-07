using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Repositories;

namespace EvacuationApi.Services;

public class RouteService : IRouteService
{
    private readonly IRouteRepository _repository;
    private readonly IReceptionPointRepository _points;
    private readonly IMapFileStorage _storage;

    public RouteService(
        IRouteRepository repository,
        IReceptionPointRepository points,
        IMapFileStorage storage)
    {
        _repository = repository;
        _points = points;
        _storage = storage;
    }

    public Task<List<EvacuationRoute>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task<EvacuationRoute?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task<EvacuationRoute> CreateAsync(RouteUpsertDto dto)
    {
        await EnsurePointExistsAsync(dto.ReceptionPointId);

        var route = MapToEntity(new EvacuationRoute(), dto);
        await _repository.AddAsync(route);
        await _repository.SaveChangesAsync();
        return route;
    }

    public async Task<bool> UpdateAsync(int id, RouteUpsertDto dto)
    {
        var route = await _repository.GetByIdAsync(id);
        if (route is null) return false;

        await EnsurePointExistsAsync(dto.ReceptionPointId);

        MapToEntity(route, dto);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var route = await _repository.GetByIdAsync(id);
        if (route is null) return false;

        var storedName = route.MapStoredName;
        _repository.Remove(route);
        await _repository.SaveChangesAsync();

        if (storedName is not null)
            _storage.Delete(storedName);
        return true;
    }

    public async Task<bool> SetMapAsync(int id, string fileName, Stream content)
    {
        var route = await _repository.GetByIdAsync(id);
        if (route is null) return false;

        var newStoredName = await _storage.SaveAsync(fileName, content);
        var oldStoredName = route.MapStoredName;

        var shownName = Path.GetFileName(fileName);
        route.MapFileName = shownName.Length > 255 ? shownName[..255] : shownName;
        route.MapStoredName = newStoredName;
        await _repository.SaveChangesAsync();

        if (oldStoredName is not null)
            _storage.Delete(oldStoredName);
        return true;
    }

    public async Task<RouteMapFileDto?> GetMapAsync(int id)
    {
        var route = await _repository.GetByIdAsync(id);
        if (route?.MapStoredName is null) return null;

        var file = _storage.Open(route.MapStoredName);
        return file is null
            ? null
            : new RouteMapFileDto(file.Content, file.ContentType, route.MapFileName ?? "map");
    }

    public async Task<bool> DeleteMapAsync(int id)
    {
        var route = await _repository.GetByIdAsync(id);
        if (route is null) return false;

        var storedName = route.MapStoredName;
        route.MapStoredName = null;
        route.MapFileName = null;
        await _repository.SaveChangesAsync();

        if (storedName is not null)
            _storage.Delete(storedName);
        return true;
    }

    private async Task EnsurePointExistsAsync(int? pointId)
    {
        if (pointId is null) return;
        if (await _points.GetByIdAsync(pointId.Value) is null)
            throw new InvalidOperationException("Указанный пункт временного размещения не найден.");
    }

    private static EvacuationRoute MapToEntity(EvacuationRoute route, RouteUpsertDto dto)
    {
        route.Name = dto.Name;
        route.Kind = dto.Kind;
        route.StartPoint = dto.StartPoint;
        route.ReceptionPointId = dto.ReceptionPointId;
        route.DistanceKm = dto.DistanceKm;
        route.TravelTimeMinutes = dto.TravelTimeMinutes;
        route.Description = dto.Description;
        return route;
    }
}
