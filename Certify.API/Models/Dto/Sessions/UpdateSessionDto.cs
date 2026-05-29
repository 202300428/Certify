namespace Certify.API.Models.Dto.Sessions;

public class UpdateSessionDto
{
    public int CourseId { get; set; }
    public int InstructorId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int Capacity { get; set; }
}
