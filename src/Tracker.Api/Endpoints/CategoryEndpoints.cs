using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Models;

namespace Tracker.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").RequireAuthorization();

        group.MapGet("/", async (HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return await db.Categories.Where(c => c.UserId == userId).ToListAsync();
        });

        group.MapGet("/{slug}", async (string slug, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug && c.UserId == userId)
                is Category category
                    ? Results.Ok(category)
                    : Results.NotFound();
        });

        group.MapPost("/", async (Category category, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            category.UserId = userId;
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return Results.Created($"/api/categories/{category.Slug}", category);
        });

        group.MapDelete("/{id}", async (int id, HttpContext http, TrackerDbContext db) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId) is Category category)
            {
                db.Categories.Remove(category);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }
            return Results.NotFound();
        });
    }
}
