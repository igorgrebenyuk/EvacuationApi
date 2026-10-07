using System.ComponentModel.DataAnnotations;

namespace EvacuationApi.Models;

/// <summary>
/// Строка транспортной ведомости: автотранспорт, выделяемый для эвакуации
/// (собственный или привлечённый по договору).
/// </summary>
public class Vehicle
{
    public int Id { get; set; }

    /// <summary>
    /// Марка / наименование, напр. «Автобус ПАЗ-3205»
    /// </summary>
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Государственный номер
    /// </summary>
    [MaxLength(20)]
    public string? PlateNumber { get; set; }

    public VehicleType Type { get; set; } = VehicleType.Bus;

    /// <summary>
    /// Пассажировместимость (количество мест)
    /// </summary>
    public int Capacity { get; set; }

    public VehicleOwnership Ownership { get; set; } = VehicleOwnership.Own;

    /// <summary>
    /// Организация-поставщик (для привлечённого транспорта)
    /// </summary>
    [MaxLength(200)]
    public string? ProviderName { get; set; }

    /// <summary>
    /// Номер договора (для привлечённого транспорта)
    /// </summary>
    [MaxLength(50)]
    public string? ContractNumber { get; set; }

    /// <summary>
    /// Срок действия договора. Не указан — договор считается бессрочным
    /// </summary>
    public DateOnly? ContractValidUntil { get; set; }

    [MaxLength(150)]
    public string? DriverName { get; set; }

    [MaxLength(30)]
    public string? DriverPhone { get; set; }

    public VehicleStatus Status { get; set; } = VehicleStatus.Ready;

    /// <summary>
    /// Договор на привлечённый транспорт истёк
    /// </summary>
    public bool IsContractExpired =>
        Ownership == VehicleOwnership.Contracted &&
        ContractValidUntil.HasValue &&
        ContractValidUntil.Value < DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Транспорт можно использовать в расчётах: он исправен и (для привлечённого) договор не истёк.
    /// </summary>
    public bool IsAvailableForEvacuation =>
        Status == VehicleStatus.Ready && !IsContractExpired;
}
