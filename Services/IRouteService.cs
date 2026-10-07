using EvacuationApi.DTOs;
using EvacuationApi.Models;

namespace EvacuationApi.Services;

public interface IRouteService
{
    Task<List<EvacuationRoute>> GetAllAsync();
    Task<EvacuationRoute?> GetByIdAsync(int id);

    /// <summary>
    /// Бросает InvalidOperationException, если указанный пункт размещения не найден
    /// </summary>
    Task<EvacuationRoute> CreateAsync(RouteUpsertDto dto);
    Task<bool> UpdateAsync(int id, RouteUpsertDto dto);
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Загружает (заменяет) карту маршрута. false — маршрут не найден
    /// </summary>
    Task<bool> SetMapAsync(int id, string fileName, Stream content);

    /// <summary>
    /// Карта маршрута; null — маршрута или карты нет
    /// </summary>
    Task<RouteMapFileDto?> GetMapAsync(int id);

    Task<bool> DeleteMapAsync(int id);
}
