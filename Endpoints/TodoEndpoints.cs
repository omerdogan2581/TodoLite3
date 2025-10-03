using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TodoLite.Data;
using TodoLite.Models;

namespace TodoLite.Endpoints
{
    public static class TodoEndpoints
    {
        public static void MapTodoEndpoints(this WebApplication app)
        {
            // Middleware benzeri: sadece giriş yapanın erişmesi
            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api/todos"))
                {
                    if (!ctx.Request.Cookies.TryGetValue("uid", out var uid) || string.IsNullOrEmpty(uid))
                    {
                        ctx.Response.StatusCode = 401;
                        await ctx.Response.WriteAsync("Not authorized");
                        return;
                    }
                    ctx.Items["uid"] = uid;
                }
                await next();
            });

            // GET - Kullanıcının tüm todoları (sadece filtre)
            app.MapGet("/api/todos", async (
                HttpContext ctx,
                AppDbContext db,
                string? search = null,
                string? status = null,
                DateTime? startDate = null,
                DateTime? endDate = null
            ) =>
            {
                var uid = ctx.Items["uid"]?.ToString();
                if (uid == null) return Results.Unauthorized();

                var query = db.Todos.AsQueryable()
                                    .Where(t => t.CreatorUserId == uid);

                // Filtreler
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(t => t.Text.Contains(search));

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(t => t.Status == status);

                if (startDate.HasValue)
                    query = query.Where(t => t.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.CreatedAt <= endDate.Value);

                var items = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                return Results.Ok(items); // sadece liste döndürüyoruz
            });


            // POST - Yeni todo oluştur
            app.MapPost("/api/todos", async (HttpContext ctx, AppDbContext db) =>
            {
                var uid = ctx.Items["uid"]?.ToString();
                var t = await JsonSerializer.DeserializeAsync<TodoItem>(ctx.Request.Body);
                if (t == null) return Results.BadRequest();

                t.Id = Guid.NewGuid();
                t.CreatorUserId = uid;
                t.CreatedAt = DateTime.UtcNow;

                db.Todos.Add(t);
                await db.SaveChangesAsync();

                return Results.Ok(t);
            });

            // PUT - Todo güncelle
            app.MapPut("/api/todos/{id:guid}", async (HttpContext ctx, Guid id, AppDbContext db) =>
            {
                var uid = ctx.Items["uid"]?.ToString();
                var input = await JsonSerializer.DeserializeAsync<TodoItem>(ctx.Request.Body);
                if (input == null) return Results.BadRequest();

                var t = await db.Todos.FirstOrDefaultAsync(x => x.Id == id && x.CreatorUserId == uid);
                if (t == null) return Results.NotFound();

                if (!string.IsNullOrWhiteSpace(input.Text)) t.Text = input.Text;
                if (!string.IsNullOrWhiteSpace(input.Status)) t.Status = input.Status;

                await db.SaveChangesAsync();
                return Results.Ok(t);
            });

            // DELETE - Todo sil
            app.MapDelete("/api/todos/{id:guid}", async (HttpContext ctx, Guid id, AppDbContext db) =>
            {
                var uid = ctx.Items["uid"]?.ToString();
                var t = await db.Todos.FirstOrDefaultAsync(x => x.Id == id && x.CreatorUserId == uid);
                if (t == null) return Results.NotFound();

                db.Todos.Remove(t);
                await db.SaveChangesAsync();
                return Results.Ok();
            });
        }
    }
}
