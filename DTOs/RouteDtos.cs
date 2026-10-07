using System.ComponentModel.DataAnnotations;
using EvacuationApi.Models;

namespace EvacuationApi.DTOs;

/// <summary>
/// Данные для создания / обновления маршрута (карта загружается отдельным запросом).
/// </summary>
public class RouteUpsertDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public RouteKind Kind { get; set; } = RouteKind.Main;

    [MaxLength(300)]
    public string? StartPoint { get; set; }

    public int? ReceptionPointId { get; set; }

    [Range(0, 5000)]
    public double DistanceKm { get; set; }

    [Range(0, 10000)]
    public int TravelTimeMinutes { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}

/// <summary>
/// Файл карты маршрута для скачивания / просмотра.
/// </summary>
public record RouteMapFileDto(Stream Content, string ContentType, string FileName);
