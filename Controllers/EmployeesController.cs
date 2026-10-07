using Microsoft.AspNetCore.Mvc;
using EvacuationApi.DTOs;
using EvacuationApi.Models;
using EvacuationApi.Services;

namespace EvacuationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    /// <summary>
    /// Список сотрудников. Фильтры: номер смены, только подлежащие эвакуации.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAll(
        [FromQuery] int? shift, [FromQuery] bool subjectOnly = false)
    {
        return Ok(await _service.GetAllAsync(shift, subjectOnly));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Employee>> GetById(int id)
    {
        var employee = await _service.GetByIdAsync(id);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> Create(EmployeeUpsertDto dto)
    {
        var employee = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EmployeeUpsertDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// Ручное назначение сотрудника в транспорт и/или пункт размещения (null — снять назначение).
    /// Проверяет доступность транспорта и наличие свободных мест.
    /// </summary>
    [HttpPatch("{id:int}/assignment")]
    public async Task<IActionResult> Assign(int id, EmployeeAssignmentDto dto)
    {
        try
        {
            var ok = await _service.AssignAsync(id, dto);
            return ok ? NoContent() : NotFound();
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
