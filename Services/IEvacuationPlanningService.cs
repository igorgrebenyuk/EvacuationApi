using EvacuationApi.DTOs;

namespace EvacuationApi.Services;

public interface IEvacuationPlanningService
{
    /// <summary>
    /// Расчётные таблицы: персонал, транспорт, размещение, маршруты, дефициты и предупреждения
    /// </summary>
    Task<EvacuationSummaryDto> GetSummaryAsync();

    /// <summary>
    /// Текущий план: кто в каком транспорте и пункте размещения
    /// </summary>
    Task<EvacuationPlanDto> GetPlanAsync();

    /// <summary>
    /// Автоматически распределяет сотрудников по транспорту и пунктам размещения
    /// (старые назначения сбрасываются). shift == null — все смены.
    /// Бросает InvalidOperationException, если эвакуировать некого.
    /// </summary>
    Task<EvacuationPlanDto> BuildAutoPlanAsync(int? shift);

    /// <summary>
    /// Снимает все назначения
    /// </summary>
    Task ResetPlanAsync();
}
