using Microsoft.EntityFrameworkCore;
using Mt.Api.Data;

namespace Mt.Api;

public static class ToDoEndpoints {


    public sealed record TodoDto(Guid Id, string Title, bool IsDone, DateTime CreatedUtc);
    public sealed record CreateTodoRequest(string Title);
    public sealed record UpdateTodoRequest(string? Title, bool? IsDone);

    
    public static IEndpointRouteBuilder MapToDoEndpoints(this IEndpointRouteBuilder app) {
        var group = app.MapGroup("/todos");

        group.MapGet("/", async (AppDbContext db, CancellationToken ct) => {
            var items = await db.Todos.AsNoTracking()
            .OrderByDescending(x => x.CreatedUtc)
            .Select(x => new { x.Id, x.Title, x.IsDone })
            .ToListAsync(ct);
            return Results.Ok(items);
        });

        group.MapPost("/", async (CreateTodoRequest request, AppDbContext db, CancellationToken ct) => {
            
            if (string.IsNullOrWhiteSpace(request.Title)) {
                return Results.BadRequest(new { error = "Title is required" });
            }

            var item = new TodoItem {
                Title = request.Title,
                IsDone = false,
                CreatedUtc = DateTime.UtcNow
            };
            db.Todos.Add(item);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/todos/{item.Id}", item);
        });

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) => {
            // IMPORTANT: do NOT use Find/FindAsync here (boss fight incoming).
            var item = await db.Todos.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

            return item is null ? Results.NotFound() : Results.Ok(new TodoDto(item.Id, item.Title, item.IsDone, item.CreatedUtc));
        });
        
        group.MapPut("/{id:guid}", async (Guid id, UpdateTodoRequest request, AppDbContext db, CancellationToken ct) => {
            var item = await db.Todos.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (item is null) {
                return Results.NotFound();
            }

            var noUpdateSupplied = string.IsNullOrWhiteSpace(request.Title) && request.IsDone is null;
            if (noUpdateSupplied) {
                return Results.BadRequest(new { error = "Provide Title and/or IsDone" });
            }

            if (!string.IsNullOrWhiteSpace(request.Title)) {
                item.Title = request.Title!;
            }
            if (request.IsDone is not null) {
                item.IsDone = request.IsDone.Value;
            }
            await db.SaveChangesAsync(ct);
            return Results.Ok(new TodoDto(item.Id, item.Title, item.IsDone, item.CreatedUtc));
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) => {
            var item = await db.Todos.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (item is null) {
                return Results.NotFound();
            }
            db.Todos.Remove(item);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        return app;
    }

    
}
