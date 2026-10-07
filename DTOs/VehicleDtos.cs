using System.ComponentModel.DataAnnotations;
using EvacuationApi.Models;

namespace EvacuationApi.DTOs;

/// <summary>
/// Данные для создания / обновления строки транспортной ведомости
/// </summary>
public class VehicleUpsertDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PlateNumber { get; set; }

    public VehicleType Type { get; set; } = VehicleType.Bus;

    [Range(1, 200)]
    public int Capacity { get; set; }

    public VehicleOwnership Ownership { get; set; } = VehicleOwnership.Own;

    [MaxLength(200)]
    public string? ProviderName { get; set; }

    [MaxLength(50)]
    public string? ContractNumber { get; set; }

    public DateOnly? ContractValidUntil { get; set; }

    [MaxLength(150)]
    public string? DriverName { get; set; }

    [MaxLength(30)]
    public string? DriverPhone { get; set; }

    public VehicleStatus Status { get; set; } = VehicleStatus.Ready;
}
