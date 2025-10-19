namespace EventBookingAPI.Models;

public class Booking
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string UserName { get; set; } = null!;
    public int Tickets { get; set; }
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
