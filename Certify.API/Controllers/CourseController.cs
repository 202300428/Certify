using Certify.API.Data;
using Certify.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Controllers;

/// <summary>
/// Manages course catalog operations. Read-only for authenticated users; creation restricted to Training Coordinators.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CoursesController(CertifyDbContext context) : ControllerBase
{

    /// <summary>Retrieves the complete course catalog with prerequisite references.</summary>
    /// <returns>List of all courses.</returns>
    /// <response code="200">Course catalog retrieved.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Ok(await context.Courses
        .Include(c => c.PrerequisiteCourse)
        .ToListAsync());

    /// <summary>Gets detailed information for a specific course, including scheduled sessions, instructor, and room.</summary>
    /// <param name="id">Unique course identifier.</param>
    /// <returns>Course details or 404 if not found.</returns>
    /// <response code="200">Course found.</response>
    /// <response code="404">Course does not exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var course = await context.Courses
            .Include(c => c.ScheduledSessions)
                .ThenInclude(ss => ss.Instructor)
            .Include(c => c.ScheduledSessions)
                .ThenInclude(ss => ss.Room)
            .FirstOrDefaultAsync(c => c.Id == id);
        return course == null ? NotFound() : Ok(course);
    }

    /// <summary>Creates a new course in the catalog. Restricted to Training Coordinators.</summary>
    /// <param name="course">Course entity containing title, category, capacity, fee, and duration.</param>
    /// <returns>The newly created course resource.</returns>
    /// <response code="201">Course created successfully.</response>
    /// <response code="400">Invalid course data.</response>
    /// <response code="403">User lacks TrainingCoordinator role.</response>
    [HttpPost]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] Course course)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        context.Courses.Add(course);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }
}