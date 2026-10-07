using Microsoft.AspNetCore.Mvc;
using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Services;

namespace EvacuationApi.Controllers;

/// <summary>
/// Пункты временного размещения (приёмные пункты, ППЭ) в безопасном районе
/// </summary>
[ApiController]
[Route("api/reception-points")]
public class ReceptionPointsController : ControllerBase
{
    private readonly IReceptionPointService _service;

    public ReceptionPointsController(IReceptionPointService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReceptionPoint>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReceptionPoint>> GetById(int id)
    {
        var point = await _service.GetByIdAsync(id);
        return point is null ? NotFound() : Ok(point);
    }

    [HttpPost]
    public async Task<ActionResult<ReceptionPoint>> Create(ReceptionPointUpsertDto dto)
    {
        var point = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = point.Id }, point);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ReceptionPointUpsertDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
