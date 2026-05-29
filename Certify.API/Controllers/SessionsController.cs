using Certify.API.Data;
using Certify.API.Models;
using Certify.API.Models.Dto.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SessionsController(CertifyDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var sessions = await context.ScheduledSessions
            .AsNoTracking()
            .Include(s => s.Course)
            .Include(s => s.Instructor)
            .Include(s => s.Room)
            .Include(s => s.Enrollments)
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable()
    {
        var sessions = await context.ScheduledSessions
            .AsNoTracking()
            .Include(s => s.Course)
            .Include(s => s.Instructor)
            .Include(s => s.Room)
            .Where(s => s.StartDateTime > DateTime.UtcNow)
            .Select(s => new
            {
                s.Id,
                s.CourseId,
                CourseTitle = s.Course.Title,
                CourseCategory = s.Course.Category,
                CourseFee = s.Course.Fee,
                s.InstructorId,
                InstructorName = s.Instructor.Name,
                s.RoomId,
                RoomName = s.Room.Name,
                s.StartDateTime,
                s.EndDateTime,
                s.Capacity,
                EnrolledCount = s.Enrollments.Count,
                AvailableSpots = s.Capacity - s.Enrollments.Count
            })
            .Where(s => s.AvailableSpots > 0)
            .OrderBy(s => s.StartDateTime)
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var session = await context.ScheduledSessions
            .AsNoTracking()
            .Include(s => s.Course)
            .Include(s => s.Instructor)
            .Include(s => s.Room)
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == id);

        return session == null ? NotFound() : Ok(session);
    }

    [HttpPost]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSessionDto dto)
    {
        if (dto.EndDateTime <= dto.StartDateTime)
            return BadRequest("End time must be after start time.");

        if (!await context.Courses.AnyAsync(c => c.Id == dto.CourseId))
            return BadRequest("Course not found.");

        if (!await context.Instructors.AnyAsync(i => i.Id == dto.InstructorId))
            return BadRequest("Instructor not found.");

        if (!await context.Rooms.AnyAsync(r => r.Id == dto.RoomId))
            return BadRequest("Room not found.");

        var session = new ScheduledSession
        {
            CourseId = dto.CourseId,
            InstructorId = dto.InstructorId,
            RoomId = dto.RoomId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            Capacity = dto.Capacity
        };

        context.ScheduledSessions.Add(session);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSessionDto dto)
    {
        var session = await context.ScheduledSessions.FindAsync(id);
        if (session == null) return NotFound();

        if (dto.EndDateTime <= dto.StartDateTime)
            return BadRequest("End time must be after start time.");

        session.CourseId = dto.CourseId;
        session.InstructorId = dto.InstructorId;
        session.RoomId = dto.RoomId;
        session.StartDateTime = dto.StartDateTime;
        session.EndDateTime = dto.EndDateTime;
        session.Capacity = dto.Capacity;

        await context.SaveChangesAsync();
        return Ok(session);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var session = await context.ScheduledSessions.FindAsync(id);
        if (session == null) return NotFound();

        context.ScheduledSessions.Remove(session);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
