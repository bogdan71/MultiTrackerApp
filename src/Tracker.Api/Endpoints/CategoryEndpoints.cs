
using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Models;

namespace Tracker.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories");

        group.MapGet("/", async (TrackerDbContext db) =>
        {
            return await db.Categories.ToListAsync();
        });

        group.MapGet("/{slug}", async (string slug, TrackerDbContext db) =>
        {
            return await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug)
                is Category category
                    ? Results.Ok(category)
                    : Results.NotFound();
        });

        group.MapPost("/", async (Category category, TrackerDbContext db) =>
        {
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return Results.Created($"/api/categories/{category.Slug}", category);
        });

        group.MapDelete("/{id}", async (int id, TrackerDbContext db) =>
        {
            if (await db.Categories.FindAsync(id) is Category category)
            {
                db.Categories.Remove(category);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }
            return Results.NotFound();
        });
    }
}
