using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;

namespace Tracker.Api.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard").RequireAuthorization();

        group.MapGet("/", async (HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var books = await db.Books.Where(b => b.UserId == userId).GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() }).ToListAsync();
            var movies = await db.Movies.Where(m => m.UserId == userId).GroupBy(m => m.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() }).ToListAsync();
            var songs = await db.Songs.Where(s => s.UserId == userId).GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() }).ToListAsync();
            var todos = new
            {
                Total = await db.TodoItems.CountAsync(t => t.UserId == userId),
                Completed = await db.TodoItems.CountAsync(t => t.UserId == userId && t.IsCompleted),
                Pending = await db.TodoItems.CountAsync(t => t.UserId == userId && !t.IsCompleted),
                HighPriority = await db.TodoItems.CountAsync(t => t.UserId == userId && !t.IsCompleted && t.Priority >= Models.Priority.High)
            };

            // Dynamic Categories Stats
            var categories = await db.Categories.Where(c => c.UserId == userId).Select(c => new
            {
                c.Name,
                c.Icon,
                c.Slug,
                Count = db.Items.Count(i => i.CategoryId == c.Id)
            }).ToListAsync();

            // Recent Dynamic Items
            var recentItems = await db.Items
                .Include(i => i.Category)
                .Where(i => i.Category!.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .Select(i => new
                {
                    i.Id,
                    i.Title,
                    i.Status,
                    CategoryName = i.Category!.Name,
                    CategorySlug = i.Category.Slug,
                    i.CreatedAt
                })
                .ToListAsync();

            var upcomingBooks = await db.Books.Where(b => b.UserId == userId && b.Status == Models.TrackingStatus.Upcoming)
                .OrderBy(b => b.ReleaseDate).Take(5).ToListAsync();
            var upcomingMovies = await db.Movies.Where(m => m.UserId == userId && m.Status == Models.TrackingStatus.Upcoming)
                .OrderBy(m => m.ReleaseDate).Take(5).ToListAsync();
            var upcomingSongs = await db.Songs.Where(s => s.UserId == userId && s.Status == Models.TrackingStatus.Upcoming)
                .OrderBy(s => s.ReleaseDate).Take(5).ToListAsync();
            var pendingTodos = await db.TodoItems.Where(t => t.UserId == userId && !t.IsCompleted)
                .OrderByDescending(t => t.Priority).Take(5).ToListAsync();

            return Results.Ok(new
            {
                Summary = new { Books = books, Movies = movies, Songs = songs, Todos = todos, Categories = categories },
                Upcoming = new { Books = upcomingBooks, Movies = upcomingMovies, Songs = upcomingSongs, RecentItems = recentItems },
                PendingTodos = pendingTodos
            });
        });

        return group;
    }
}
