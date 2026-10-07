using Microsoft.AspNetCore.Mvc;
using EvacuationApi.DTOs;
using EvacuationApi.Services;

namespace EvacuationApi.Controllers;

/// <summary>Расчёты и планирование эвакуации.</summary>
[ApiController]
[Route("api/evacuation")]
public class EvacuationController : ControllerBase
{
    private readonly IEvacuationPlanningService _service;

    public EvacuationController(IEvacuationPlanningService service)
    {
        _service = service;
    }

    /// <summary>
    /// Расчётные таблицы: сколько людей подлежит эвакуации (всего / по сменам),
    /// сколько мест в транспорте и пунктах размещения, дефициты, предупреждения.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<EvacuationSummaryDto>> GetSummary()
    {
        return Ok(await _service.GetSummaryAsync());
    }

    /// <summary>Текущий план: распределение по транспорту и пунктам размещения.</summary>
    [HttpGet("plan")]
    public async Task<ActionResult<EvacuationPlanDto>> GetPlan()
    {
        return Ok(await _service.GetPlanAsync());
    }

    /// <summary>
    /// Автоматически распределить сотрудников по транспорту и пунктам размещения.
    /// Без параметра shift — все смены, с shift=N — только указанная смена.
    /// Прежние назначения сбрасываются.
    /// </summary>
    [HttpPost("plan/auto")]
    public async Task<ActionResult<EvacuationPlanDto>> BuildAutoPlan([FromQuery] int? shift)
    {
        try
        {
            return Ok(await _service.BuildAutoPlanAsync(shift));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Снять все назначения.
    /// </summary>
    [HttpDelete("plan")]
    public async Task<IActionResult> ResetPlan()
    {
        await _service.ResetPlanAsync();
        return NoContent();
    }
}
