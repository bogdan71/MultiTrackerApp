using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Models;

namespace Tracker.Api.Endpoints;

public static class SongEndpoints
{
    public static RouteGroupBuilder MapSongEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/songs").WithTags("Songs");

        group.MapGet("/", async (TrackerDbContext db, string? status, string? genre) =>
        {
            var query = db.Songs.AsQueryable();
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TrackingStatus>(status, true, out var s))
                query = query.Where(x => x.Status == s);
            if (!string.IsNullOrEmpty(genre))
                query = query.Where(x => x.Genre != null && x.Genre.ToLower().Contains(genre.ToLower()));
            return Results.Ok(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
        });

        group.MapGet("/{id:int}", async (int id, TrackerDbContext db) =>
        {
            var song = await db.Songs.FindAsync(id);
            return song is not null ? Results.Ok(song) : Results.NotFound();
        });

        group.MapPost("/", async (Song song, TrackerDbContext db) =>
        {
            song.CreatedAt = DateTime.UtcNow;
            song.UpdatedAt = DateTime.UtcNow;
            db.Songs.Add(song);
            await db.SaveChangesAsync();
            return Results.Created($"/api/songs/{song.Id}", song);
        });

        group.MapPut("/{id:int}", async (int id, Song input, TrackerDbContext db) =>
        {
            var song = await db.Songs.FindAsync(id);
            if (song is null) return Results.NotFound();

            song.Title = input.Title;
            song.Artist = input.Artist;
            song.Album = input.Album;
            song.Genre = input.Genre;
            song.ReleaseDate = input.ReleaseDate;
            song.Status = input.Status;
            song.Notes = input.Notes;
            song.AlbumArtUrl = input.AlbumArtUrl;
            song.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(song);
        });

        group.MapDelete("/{id:int}", async (int id, TrackerDbContext db) =>
        {
            var song = await db.Songs.FindAsync(id);
            if (song is null) return Results.NotFound();
            db.Songs.Remove(song);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return group;
    }
}
