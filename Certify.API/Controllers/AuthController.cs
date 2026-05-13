using Certify.API.Models;
using Certify.API.Models.Dto.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Certify.API.Controllers;

/// <summary>
/// Handles user registration, authentication, and JWT token issuance.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config) : ControllerBase
{

    /// <summary>Registers a new user and assigns the specified role.</summary>
    /// <param name="dto">Registration payload containing email, password, role, and optional full name.</param>
    /// <returns>Success message on creation, or validation errors.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Invalid payload or role does not exist.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!await roleManager.RoleExistsAsync(dto.Role)) return BadRequest("Invalid role.");

        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName ?? dto.Email };
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        await userManager.AddToRoleAsync(user, dto.Role);
        return Ok(new { Message = "User registered successfully." });
    }

    /// <summary>Authenticates credentials and returns a JWT token.</summary>
    /// <param name="dto">Login credentials (email + password).</param>
    /// <returns>JWT token, user email, role, and expiration time.</returns>
    /// <response code="200">Successful authentication.</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles.First());
        return Ok(new AuthResponseDto { Token = token, Email = user.Email!, Role = roles.First(), ExpiresAt = DateTime.UtcNow.AddHours(1) });
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}