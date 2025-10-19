using Microsoft.EntityFrameworkCore;
using EventBookingAPI.Data;
using EventBookingAPI.DTOs;
using EventBookingAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Booking API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// --- Event Endpoints ---
app.MapGet("/api/events", async (AppDbContext db) =>
{
    var events = await db.Events
        .Include(e => e.Bookings)
        .Where(e => e.Date >= DateTime.UtcNow)
        .ToListAsync();
    return Results.Ok(events);
});

app.MapPost("/api/events", async (EventDto dto, AppDbContext db) =>
{
    var ev = new Event
    {
        Name = dto.Name,
        Location = dto.Location,
        Date = dto.Date,
        Capacity = dto.Capacity
    };

    db.Events.Add(ev);
    await db.SaveChangesAsync();
    return Results.Created($"/api/events/{ev.Id}", ev);
});

// --- Booking Endpoint ---
app.MapPost("/api/events/{id}/book", async (int id, BookingDto dto, AppDbContext db) =>
{
    var ev = await db.Events.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.Id == id);
    if (ev == null) return Results.NotFound("Event not found");

    var bookedTickets = ev.Bookings.Sum(b => b.Tickets);
    if (bookedTickets + dto.Tickets > ev.Capacity)
        return Results.BadRequest("Not enough capacity for this booking");

    var booking = new Booking
    {
        EventId = id,
        UserName = dto.UserName,
        Tickets = dto.Tickets
    };

    db.Bookings.Add(booking);
    await db.SaveChangesAsync();
    return Results.Created($"/api/bookings/{booking.Id}", booking);
});

// --- User bookings ---
app.MapGet("/api/users/{userName}/bookings", async (string userName, AppDbContext db) =>
{
    var bookings = await db.Bookings
        .Include(b => b.Event)
        .Where(b => b.UserName == userName)
        .ToListAsync();

    return Results.Ok(bookings);
});

app.Run();
