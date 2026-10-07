using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EvacuationApi.Models;

/// <summary>
/// Маршрут эвакуации в загородную зону (безопасный район).
/// К маршруту можно прикрепить электронную карту (PDF / PNG / JPG).
/// </summary>
public class EvacuationRoute
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public RouteKind Kind { get; set; } = RouteKind.Main;

    /// <summary>Пункт отправления (адрес / место сбора).</summary>
    [MaxLength(300)]
    public string? StartPoint { get; set; }

    /// <summary>Пункт временного размещения, куда ведёт маршрут (null — не задан).</summary>
    public int? ReceptionPointId { get; set; }

    /// <summary>Протяжённость, км.</summary>
    public double DistanceKm { get; set; }

    /// <summary>Расчётное время в пути, мин.</summary>
    public int TravelTimeMinutes { get; set; }

    /// <summary>Описание: основные участки, ориентиры, контрольные пункты.</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Исходное имя загруженного файла карты.</summary>
    [MaxLength(255)]
    public string? MapFileName { get; set; }

    /// <summary>Имя файла карты на диске. Наружу не отдаётся.</summary>
    [JsonIgnore, MaxLength(100)]
    public string? MapStoredName { get; set; }

    /// <summary>К маршруту прикреплена электронная карта.</summary>
    public bool HasMap => !string.IsNullOrEmpty(MapStoredName);
}
