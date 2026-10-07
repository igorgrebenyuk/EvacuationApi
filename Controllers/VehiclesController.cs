using Microsoft.AspNetCore.Mvc;
using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Services;

namespace EvacuationApi.Controllers;

/// <summary>
/// Транспортная ведомость: реестр автотранспорта (собственный + привлечённый по договорам).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _service;

    public VehiclesController(IVehicleService service)
    {
        _service = service;
    }

    /// <summary>
    /// Список транспорта. Необязательный фильтр по принадлежности (Own / Contracted).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetAll([FromQuery] VehicleOwnership? ownership)
    {
        return Ok(await _service.GetAllAsync(ownership));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Vehicle>> GetById(int id)
    {
        var vehicle = await _service.GetByIdAsync(id);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<ActionResult<Vehicle>> Create(VehicleUpsertDto dto)
    {
        try
        {
            var vehicle = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, VehicleUpsertDto dto)
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
