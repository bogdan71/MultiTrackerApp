
using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Models;

namespace Tracker.Api.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories/{slug}/items");

        group.MapGet("/", async (string slug, TrackerDbContext db) =>
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (category == null) return Results.NotFound("Category not found");

            var items = await db.Items
                .Where(i => i.CategoryId == category.Id)
                .ToListAsync();

            return Results.Ok(items);
        });

        group.MapPost("/", async (string slug, Item item, TrackerDbContext db) =>
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (category == null) return Results.NotFound("Category not found");

            item.CategoryId = category.Id;
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/categories/{slug}/items/{item.Id}", item);
        });

        group.MapPut("/{id}", async (string slug, int id, Item updatedItem, TrackerDbContext db) =>
        {
             var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (category == null) return Results.NotFound("Category not found");

            var item = await db.Items.FindAsync(id);
            if (item is null || item.CategoryId != category.Id) return Results.NotFound();

            item.Title = updatedItem.Title;
            item.Description = updatedItem.Description;
            item.Status = updatedItem.Status;
            item.Properties = updatedItem.Properties;
            item.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (string slug, int id, TrackerDbContext db) =>
        {
             var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
            if (category == null) return Results.NotFound("Category not found");

            var item = await db.Items.FindAsync(id);
            if (item is null || item.CategoryId != category.Id) return Results.NotFound();

            db.Items.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
