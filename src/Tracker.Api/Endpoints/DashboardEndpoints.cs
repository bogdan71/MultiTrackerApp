using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;

namespace Tracker.Api.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group.MapGet("/", async (TrackerDbContext db) =>
        {
            var books = await db.Books.GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() }).ToListAsync();
            var movies = await db.Movies.GroupBy(m => m.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() }).ToListAsync();
            var songs = await db.Songs.GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() }).ToListAsync();
            var todos = new
            {
                Total = await db.TodoItems.CountAsync(),
                Completed = await db.TodoItems.CountAsync(t => t.IsCompleted),
                Pending = await db.TodoItems.CountAsync(t => !t.IsCompleted),
                HighPriority = await db.TodoItems.CountAsync(t => !t.IsCompleted && t.Priority >= Models.Priority.High)
            };

            var upcomingBooks = await db.Books.Where(b => b.Status == Models.TrackingStatus.Upcoming)
                .OrderBy(b => b.ReleaseDate).Take(5).ToListAsync();
            var upcomingMovies = await db.Movies.Where(m => m.Status == Models.TrackingStatus.Upcoming)
                .OrderBy(m => m.ReleaseDate).Take(5).ToListAsync();
            var upcomingSongs = await db.Songs.Where(s => s.Status == Models.TrackingStatus.Upcoming)
                .OrderBy(s => s.ReleaseDate).Take(5).ToListAsync();
            var pendingTodos = await db.TodoItems.Where(t => !t.IsCompleted)
                .OrderByDescending(t => t.Priority).Take(5).ToListAsync();

            return Results.Ok(new
            {
                Summary = new { Books = books, Movies = movies, Songs = songs, Todos = todos },
                Upcoming = new { Books = upcomingBooks, Movies = upcomingMovies, Songs = upcomingSongs },
                PendingTodos = pendingTodos
            });
        });

        return group;
    }
}
