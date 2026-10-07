using Microsoft.AspNetCore.Mvc;
using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Services;

namespace EvacuationApi.Controllers;

/// <summary>
/// Маршруты эвакуации в загородную зону и их электронные карты.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly IRouteService _service;

    public RoutesController(IRouteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EvacuationRoute>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EvacuationRoute>> GetById(int id)
    {
        var route = await _service.GetByIdAsync(id);
        return route is null ? NotFound() : Ok(route);
    }

    [HttpPost]
    public async Task<ActionResult<EvacuationRoute>> Create(RouteUpsertDto dto)
    {
        try
        {
            var route = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = route.Id }, route);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, RouteUpsertDto dto)
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

    /// <summary>Загрузить (или заменить) электронную карту маршрута. Поле формы: file. PDF / PNG / JPG, до 10 МБ.</summary>
    [HttpPost("{id:int}/map")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> UploadMap(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Файл не передан." });

        try
        {
            await using var stream = file.OpenReadStream();
            var ok = await _service.SetMapAsync(id, file.FileName, stream);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Открыть карту маршрута (отдаётся файлом для просмотра в браузере).</summary>
    [HttpGet("{id:int}/map")]
    public async Task<IActionResult> GetMap(int id)
    {
        var map = await _service.GetMapAsync(id);
        if (map is null) return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(map.Content, map.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("{id:int}/map")]
    public async Task<IActionResult> DeleteMap(int id)
    {
        var ok = await _service.DeleteMapAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
