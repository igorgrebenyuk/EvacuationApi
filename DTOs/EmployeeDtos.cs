using System.ComponentModel.DataAnnotations;

namespace EvacuationApi.DTOs;

/// <summary>
/// Данные для создания / полного обновления сотрудника.
/// </summary>
public class EmployeeUpsertDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Position { get; set; }

    [MaxLength(150)]
    public string? Department { get; set; }

    [Range(1, 5)]
    public int Shift { get; set; } = 1;

    public bool IsSubjectToEvacuation { get; set; } = true;

    public bool NeedsAssistance { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }
}

/// <summary>
/// Ручное назначение сотрудника: транспорт и/или пункт размещения (null — снять назначение).
/// </summary>
public class EmployeeAssignmentDto
{
    public int? VehicleId { get; set; }
    public int? ReceptionPointId { get; set; }
}
