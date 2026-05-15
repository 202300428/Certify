using Microsoft.AspNetCore.SignalR;

namespace Certify.Web.Hubs;

// Clients connect via /courseHub and listen for "EnrollmentCountChanged".
// Controllers push via IHubContext<CourseHub> after enrollment saves.
public class CourseHub : Hub
{
}