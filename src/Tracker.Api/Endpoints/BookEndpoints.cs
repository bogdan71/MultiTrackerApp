using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Models;

namespace Tracker.Api.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", async (TrackerDbContext db, string? status, string? genre) =>
        {
            var query = db.Books.AsQueryable();
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TrackingStatus>(status, true, out var s))
                query = query.Where(b => b.Status == s);
            if (!string.IsNullOrEmpty(genre))
                query = query.Where(b => b.Genre != null && b.Genre.ToLower().Contains(genre.ToLower()));
            return Results.Ok(await query.OrderByDescending(b => b.CreatedAt).ToListAsync());
        });

        group.MapGet("/{id:int}", async (int id, TrackerDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            return book is not null ? Results.Ok(book) : Results.NotFound();
        });

        group.MapPost("/", async (Book book, TrackerDbContext db) =>
        {
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;
            db.Books.Add(book);
            await db.SaveChangesAsync();
            return Results.Created($"/api/books/{book.Id}", book);
        });

        group.MapPut("/{id:int}", async (int id, Book input, TrackerDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
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

        group.MapDelete("/{id:int}", async (int id, TrackerDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null) return Results.NotFound();
            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return group;
    }
}
