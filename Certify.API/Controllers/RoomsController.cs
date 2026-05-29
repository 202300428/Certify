using Certify.API.Data;
using Certify.API.Models;
using Certify.API.Models.Dto.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "TrainingCoordinator")]
public class RoomsController(CertifyDbContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await context.Rooms.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return room == null ? NotFound() : Ok(room);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            Equipment = dto.Equipment
        };

        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        room.Name = dto.Name;
        room.Capacity = dto.Capacity;
        room.Equipment = dto.Equipment;

        await context.SaveChangesAsync();
        return Ok(room);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        context.Rooms.Remove(room);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
