using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Grades.Commands;
using OrderService.Application.Grades.Queries;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GradesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateGrade([FromBody] CreateGradeCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByStudent), new { studentId = command.StudentId }, new { id });
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetByStudent(string studentId, CancellationToken cancellationToken)
    {
        var grades = await mediator.Send(new GetGradesByStudentQuery(studentId), cancellationToken);
        return Ok(grades);
    }
}
