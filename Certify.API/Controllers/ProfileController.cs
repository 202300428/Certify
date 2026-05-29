using Certify.API.Models;
using Certify.API.Models.Dto.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Certify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId!);
        var roles = await userManager.GetRolesAsync(user!);

        return Ok(new ProfileDto
        {
            Id = user!.Id,
            Email = user.Email!,
            FullName = user.FullName,
            CPR = user.CPR,
            Role = roles.FirstOrDefault() ?? string.Empty,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId!);

        if (dto.FullName != null) user!.FullName = dto.FullName;
        if (dto.CPR != null) user!.CPR = dto.CPR;

        var result = await userManager.UpdateAsync(user!);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { Message = "Profile updated." });
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId!);

        var result = await userManager.ChangePasswordAsync(user!, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { Message = "Password changed." });
    }
}
