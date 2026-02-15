using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Models;

namespace Tracker.Api.Endpoints;

public static class MovieEndpoints
{
    public static RouteGroupBuilder MapMovieEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/movies").WithTags("Movies").RequireAuthorization();

        group.MapGet("/", async (HttpContext http, TrackerDbContext db, string? status, string? genre) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var query = db.Movies.Where(m => m.UserId == userId);
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TrackingStatus>(status, true, out var s))
                query = query.Where(m => m.Status == s);
            if (!string.IsNullOrEmpty(genre))
                query = query.Where(m => m.Genre != null && m.Genre.ToLower().Contains(genre.ToLower()));
            return Results.Ok(await query.OrderByDescending(m => m.CreatedAt).ToListAsync());
        });

        group.MapGet("/{id:int}", async (int id, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var movie = await db.Movies.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            return movie is not null ? Results.Ok(movie) : Results.NotFound();
        });

        group.MapPost("/", async (Movie movie, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            movie.UserId = userId;
            movie.CreatedAt = DateTime.UtcNow;
            movie.UpdatedAt = DateTime.UtcNow;
            db.Movies.Add(movie);
            await db.SaveChangesAsync();
            return Results.Created($"/api/movies/{movie.Id}", movie);
        });

        group.MapPut("/{id:int}", async (int id, Movie input, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var movie = await db.Movies.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (movie is null) return Results.NotFound();

            movie.Title = input.Title;
            movie.Director = input.Director;
            movie.Genre = input.Genre;
            movie.ReleaseDate = input.ReleaseDate;
            movie.Status = input.Status;
            movie.Notes = input.Notes;
            movie.PosterUrl = input.PosterUrl;
            movie.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(movie);
        });

        group.MapDelete("/{id:int}", async (int id, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var movie = await db.Movies.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (movie is null) return Results.NotFound();
            db.Movies.Remove(movie);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return group;
    }
}
