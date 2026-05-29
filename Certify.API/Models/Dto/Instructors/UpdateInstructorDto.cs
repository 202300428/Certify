namespace Certify.API.Models.Dto.Instructors;

public class UpdateInstructorDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ExpertiseAreas { get; set; }
}
