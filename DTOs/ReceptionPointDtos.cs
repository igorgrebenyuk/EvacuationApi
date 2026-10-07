using System.ComponentModel.DataAnnotations;

namespace EvacuationApi.DTOs;

/// <summary>
/// Данные для создания / обновления пункта временного размещения.
/// </summary>
public class ReceptionPointUpsertDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? HostOrganization { get; set; }

    [MaxLength(150)]
    public string? ContactPerson { get; set; }

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    [Range(0, 100000)]
    public int Capacity { get; set; }

    [MaxLength(50)]
    public string? AgreementNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
