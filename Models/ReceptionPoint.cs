using System.ComponentModel.DataAnnotations;

namespace EvacuationApi.Models;

/// <summary>
/// Пункт временного размещения (приёмный пункт, ППЭ) сотрудников в безопасном районе:
/// адрес здания и контакты принимающей стороны.
/// </summary>
public class ReceptionPoint
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Принимающая организация / владелец здания.
    /// </summary>
    [MaxLength(200)]
    public string? HostOrganization { get; set; }

    /// <summary>
    /// Контактное лицо принимающей стороны.
    /// </summary>
    [MaxLength(150)]
    public string? ContactPerson { get; set; }

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Вместимость — сколько человек можно разместить
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Номер соглашения о размещении
    /// </summary>
    [MaxLength(50)]
    public string? AgreementNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
