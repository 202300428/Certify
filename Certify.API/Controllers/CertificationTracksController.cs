using Certify.API.Data;
using Certify.API.Models;
using Certify.API.Models.Dto.Tracks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Controllers;

[Route("api/certification-tracks")]
[ApiController]
[Authorize]
public class CertificationTracksController(CertifyDbContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tracks = await context.CertificationTracks
            .AsNoTracking()
            .Include(t => t.TrackCourses)
                .ThenInclude(tc => tc.Course)
            .ToListAsync();

        return Ok(tracks);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var track = await context.CertificationTracks
            .AsNoTracking()
            .Include(t => t.TrackCourses)
                .ThenInclude(tc => tc.Course)
            .FirstOrDefaultAsync(t => t.Id == id);

        return track == null ? NotFound() : Ok(track);
    }

    [HttpPost]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTrackDto dto)
    {
        if (await context.CertificationTracks.AnyAsync(t => t.Name == dto.Name))
            return BadRequest("A track with this name already exists.");

        var track = new CertificationTrack
        {
            Name = dto.Name,
            Description = dto.Description
        };

        context.CertificationTracks.Add(track);
        await context.SaveChangesAsync();

        if (dto.CourseIds.Count != 0)
        {
            var courses = await context.Courses.Where(c => dto.CourseIds.Contains(c.Id)).ToListAsync();
            foreach (var course in courses)
            {
                context.TrackCourses.Add(new TrackCourse
                {
                    CertificationTrackId = track.Id,
                    CourseId = course.Id
                });
            }
            await context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetById), new { id = track.Id }, track);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTrackDto dto)
    {
        var track = await context.CertificationTracks
            .Include(t => t.TrackCourses)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (track == null) return NotFound();

        if (await context.CertificationTracks.AnyAsync(t => t.Name == dto.Name && t.Id != id))
            return BadRequest("A track with this name already exists.");

        track.Name = dto.Name;
        track.Description = dto.Description;

        context.TrackCourses.RemoveRange(track.TrackCourses);

        var courses = await context.Courses.Where(c => dto.CourseIds.Contains(c.Id)).ToListAsync();
        foreach (var course in courses)
        {
            context.TrackCourses.Add(new TrackCourse
            {
                CertificationTrackId = track.Id,
                CourseId = course.Id
            });
        }

        await context.SaveChangesAsync();
        return Ok(track);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var track = await context.CertificationTracks.FindAsync(id);
        if (track == null) return NotFound();

        context.CertificationTracks.Remove(track);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
