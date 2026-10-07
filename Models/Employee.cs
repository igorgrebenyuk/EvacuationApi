using System.ComponentModel.DataAnnotations;

namespace EvacuationApi.Models;

/// <summary>
/// Сотрудник организации в расчётах эвакуации (раздел «Эвакуационные мероприятия» Плана ГО).
/// </summary>
public class Employee
{
    public int Id { get; set; }

    /// <summary>
    /// ФИО.
    /// </summary>
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Должность
    /// </summary>
    [MaxLength(150)]
    public string? Position { get; set; }

    /// <summary>
    /// Подразделение
    /// </summary>
    [MaxLength(150)]
    public string? Department { get; set; }

    /// <summary>
    /// Номер рабочей смены (1, 2, 3 ...). Нужен для расчёта «всего / в смене»
    /// /summary>
    public int Shift { get; set; } = 1;

    /// <summary>
    /// true — сотрудник подлежит эвакуации;
    /// false — остаётся на объекте (дежурный персонал, обеспечение работы организации).
    /// </summary>
    public bool IsSubjectToEvacuation { get; set; } = true;

    /// <summary>
    /// Маломобильный сотрудник — нужна помощь при посадке/размещении
    /// </summary>
    public bool NeedsAssistance { get; set; }

    /// <summary>
    /// Контактный телефон
    /// </summary>
    [MaxLength(30)]
    public string? Phone { get; set; }

    // --- Назначения плана эвакуации ---

    /// <summary>
    /// Назначенное транспортное средство (null — не назначено)
    /// </summary>
    public int? VehicleId { get; set; }

    /// <summary>
    /// Назначенный пункт временного размещения (null — не назначен)
    /// </summary>
    public int? ReceptionPointId { get; set; }
}
