using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Models;

namespace Tracker.Api.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books").RequireAuthorization();

        group.MapGet("/", async (HttpContext http, TrackerDbContext db, string? status, string? genre) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var query = db.Books.Where(b => b.UserId == userId);
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TrackingStatus>(status, true, out var s))
                query = query.Where(b => b.Status == s);
            if (!string.IsNullOrEmpty(genre))
                query = query.Where(b => b.Genre != null && b.Genre.ToLower().Contains(genre.ToLower()));
            return Results.Ok(await query.OrderByDescending(b => b.CreatedAt).ToListAsync());
        });

        group.MapGet("/{id:int}", async (int id, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var book = await db.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            return book is not null ? Results.Ok(book) : Results.NotFound();
        });

        group.MapPost("/", async (Book book, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            book.UserId = userId;
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;
            db.Books.Add(book);
            await db.SaveChangesAsync();
            return Results.Created($"/api/books/{book.Id}", book);
        });

        group.MapPut("/{id:int}", async (int id, Book input, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var book = await db.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (book is null) return Results.NotFound();

            book.Title = input.Title;
            book.Author = input.Author;
            book.Genre = input.Genre;
            book.ReleaseDate = input.ReleaseDate;
            book.Status = input.Status;
            book.Notes = input.Notes;
            book.CoverImageUrl = input.CoverImageUrl;
            book.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(book);
        });

        group.MapDelete("/{id:int}", async (int id, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var book = await db.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (book is null) return Results.NotFound();
            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return group;
    }
}
