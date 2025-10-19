namespace EventBookingAPI.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime Date { get; set; }
    public int Capacity { get; set; }
    public List<Booking> Bookings { get; set; } = new();
}
