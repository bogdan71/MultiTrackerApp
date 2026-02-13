using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Models;

namespace Tracker.Api.Endpoints;

public static class TodoEndpoints
{
    public static RouteGroupBuilder MapTodoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/todos").WithTags("Todos");

        group.MapGet("/", async (TrackerDbContext db, string? priority, bool? completed, string? category) =>
        {
            var query = db.TodoItems.AsQueryable();
            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<Priority>(priority, true, out var p))
                query = query.Where(t => t.Priority == p);
            if (completed.HasValue)
                query = query.Where(t => t.IsCompleted == completed.Value);
            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => t.Category != null && t.Category.ToLower().Contains(category.ToLower()));
            return Results.Ok(await query.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate).ToListAsync());
        });

        group.MapGet("/{id:int}", async (int id, TrackerDbContext db) =>
        {
            var todo = await db.TodoItems.FindAsync(id);
            return todo is not null ? Results.Ok(todo) : Results.NotFound();
        });

        group.MapPost("/", async (TodoItem todo, TrackerDbContext db) =>
        {
            todo.CreatedAt = DateTime.UtcNow;
            todo.UpdatedAt = DateTime.UtcNow;
            db.TodoItems.Add(todo);
            await db.SaveChangesAsync();
            return Results.Created($"/api/todos/{todo.Id}", todo);
        });

        group.MapPut("/{id:int}", async (int id, TodoItem input, TrackerDbContext db) =>
        {
            var todo = await db.TodoItems.FindAsync(id);
            if (todo is null) return Results.NotFound();

            todo.Title = input.Title;
            todo.Description = input.Description;
            todo.DueDate = input.DueDate;
            todo.Priority = input.Priority;
            todo.IsCompleted = input.IsCompleted;
            todo.Category = input.Category;
            todo.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(todo);
        });

        group.MapPatch("/{id:int}/toggle", async (int id, TrackerDbContext db) =>
        {
            var todo = await db.TodoItems.FindAsync(id);
            if (todo is null) return Results.NotFound();
            todo.IsCompleted = !todo.IsCompleted;
            todo.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(todo);
        });

        group.MapDelete("/{id:int}", async (int id, TrackerDbContext db) =>
        {
            var todo = await db.TodoItems.FindAsync(id);
            if (todo is null) return Results.NotFound();
            db.TodoItems.Remove(todo);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return group;
    }
}
