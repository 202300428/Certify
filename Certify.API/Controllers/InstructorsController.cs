using Certify.API.Data;
using Certify.API.Models;
using Certify.API.Models.Dto.Instructors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "TrainingCoordinator")]
public class InstructorsController(CertifyDbContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await context.Instructors.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await context.Instructors.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return instructor == null ? NotFound() : Ok(instructor);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto)
    {
        if (await context.Instructors.AnyAsync(i => i.Email == dto.Email))
            return BadRequest("An instructor with this email already exists.");

        var instructor = new Instructor
        {
            Name = dto.Name,
            Email = dto.Email,
            ExpertiseAreas = dto.ExpertiseAreas
        };

        context.Instructors.Add(instructor);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInstructorDto dto)
    {
        var instructor = await context.Instructors.FindAsync(id);
        if (instructor == null) return NotFound();

        if (await context.Instructors.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            return BadRequest("An instructor with this email already exists.");

        instructor.Name = dto.Name;
        instructor.Email = dto.Email;
        instructor.ExpertiseAreas = dto.ExpertiseAreas;

        await context.SaveChangesAsync();
        return Ok(instructor);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var instructor = await context.Instructors.FindAsync(id);
        if (instructor == null) return NotFound();

        context.Instructors.Remove(instructor);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
