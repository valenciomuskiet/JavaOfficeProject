using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Models;
using StarterKit.Services;
using System.Threading.Tasks;

namespace StarterKit.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("GetAllEvents")]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpPost("{eventId}/reviews")]
        public async Task<IActionResult> PostReview(int eventId, string review)
        {
            var newReview = await _eventService.AddReviewAsync(eventId, review);
            return CreatedAtAction("PostReview", newReview);
        }

        [HttpPost("events")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEvent([FromBody] Eventbody eventBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdEvent = await _eventService.CreateEventAsync(eventBody);

            return CreatedAtAction(nameof(GetAllEvents), new { id = createdEvent.EventId }, createdEvent);
        }

        [HttpPut("UpdateEvent/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Eventbody eventBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedEvent = await _eventService.UpdateEventAsync(id, eventBody);
            if (updatedEvent == null)
            {
                return NotFound("Event not found.");
            }

            return Ok(updatedEvent);
        }

        [HttpDelete("DeleteEvent/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var deleted = await _eventService.DeleteEventAsync(id);
            if (!deleted)
            {
                return NotFound("Event not found.");
            }

            return Ok("Event deleted successfully.");
        }

        [HttpPost("Attendance")]
        public async Task<IActionResult> AddAttendance(int eventId, int userId)
        {
            var result = await _eventService.AddAttendanceAsync(eventId, userId);

            // Check the 'Success' field of the tuple
            if (!result.Success)
            {
                return BadRequest(result.Message); // Return the failure message from the service
            }

            return Ok(new
            {
                result.Message,
                result.AttendedEvent
            });
        }

        [HttpGet("{eventId}/attendees")]
        [Authorize] // Ensures only authenticated users can access this route
        public async Task<IActionResult> GetEventAttendees(int eventId)
        {
            var eventAttendees = await _eventService.GetEventAttendeesAsync(eventId);

            if (eventAttendees == null)
            {
                return NotFound("Event not found.");
            }

            // Return a list of users (attendees) attending the event
            return Ok(eventAttendees.Select(ea => new
            {
                ea.user.UserId,
                ea.user.FirstName,
                ea.user.LastName,
                ea.user.Email
            }));
        }

        [HttpDelete("{eventId}/attendees/{userId}")]
        [Authorize]
        public async Task<IActionResult> DeleteEventAttendee(int eventId, int userId)
        {
            var deleted = await _eventService.DeleteEventAsync(eventId);
            if (!deleted)
            {
                return NotFound("Event or attendee not found.");
            }

            return Ok("Attendee removed successfully.");
        }
    }

}

public class Eventbody
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public required string Location { get; set; }
}