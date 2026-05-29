namespace Certify.API.Models.Dto.Tracks;

public class UpdateTrackDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> CourseIds { get; set; } = [];
}
