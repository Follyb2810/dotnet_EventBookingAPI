namespace EventBookingAPI.DTOs;

public record EventDto(string Name, string Location, DateTime Date, int Capacity);
