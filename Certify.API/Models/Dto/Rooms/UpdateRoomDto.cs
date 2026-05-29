namespace Certify.API.Models.Dto.Rooms;

public class UpdateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Equipment { get; set; }
}
