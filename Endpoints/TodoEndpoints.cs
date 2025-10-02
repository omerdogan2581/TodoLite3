using System.Text.Json;
using TodoLite.Models;
using TodoLite.Services;

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

            // GET
            app.MapGet("/api/todos", async (HttpContext ctx, TodoService todoService) =>
            {
                var uid = ctx.Items["uid"]?.ToString();
                var list = await todoService.LoadTodos();
                return list.Where(t => t.CreatorUserId == uid);
            });

            // POST
            app.MapPost("/api/todos", async (HttpContext ctx, TodoService todoService) =>
            {
                var uid = ctx.Items["uid"]?.ToString();
                var t = await JsonSerializer.DeserializeAsync<TodoItem>(ctx.Request.Body);
                if (t == null) return Results.BadRequest();

                t.Id = Guid.NewGuid();
                t.CreatorUserId = uid;
                t.CreatedAt = DateTime.UtcNow;

                var list = await todoService.LoadTodos();
                list.Add(t);
                await todoService.SaveTodos(list);

                return Results.Ok(t);
            });

            // PUT
            app.MapPut("/api/todos/{id:guid}", async (HttpContext ctx, Guid id, TodoService todoService) =>
            {
                var uid = ctx.Items["uid"]?.ToString();
                var input = await JsonSerializer.DeserializeAsync<TodoItem>(ctx.Request.Body);
                if (input == null) return Results.BadRequest();

                var list = await todoService.LoadTodos();
                var t = list.FirstOrDefault(x => x.Id == id && x.CreatorUserId == uid);
                if (t == null) return Results.NotFound();

                if (!string.IsNullOrWhiteSpace(input.Text)) t.Text = input.Text;
                if (!string.IsNullOrWhiteSpace(input.Status)) t.Status = input.Status;

                await todoService.SaveTodos(list);
                return Results.Ok(t);
            });

            // DELETE
            app.MapDelete("/api/todos/{id:guid}", async (HttpContext ctx, Guid id, TodoService todoService) =>
            {
                var uid = ctx.Items["uid"]?.ToString();
                var list = await todoService.LoadTodos();
                var t = list.FirstOrDefault(x => x.Id == id && x.CreatorUserId == uid);
                if (t == null) return Results.NotFound();

                list.Remove(t);
                await todoService.SaveTodos(list);
                return Results.Ok();
            });
        }
    }
}
