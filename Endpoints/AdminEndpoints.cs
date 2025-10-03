using Microsoft.EntityFrameworkCore;
using TodoLite.Data;
using TodoLite.Models;
using TodoLite.Services;

namespace TodoLite.Endpoints
{
    public static class AdminEndpoints
    {
        public static void MapAdminEndpoints(this WebApplication app)
        {
            // Kullanıcıları listele
            app.MapGet("/api/admin/users", async (HttpContext ctx, AppDbContext db) =>
            {
                var uid = ctx.Request.Cookies["uid"];
                var current = await db.Users.FirstOrDefaultAsync(u => u.Id == uid);

                if (current == null || current.Role != "Admin")
                    return Results.Forbid();

                var users = await db.Users.ToListAsync();
                return Results.Ok(users);
            });

            // Kullanıcı güncelleme endpoint
            app.MapPut("/api/admin/users/{id}", async (string id, UserUpdateDto input, HttpContext ctx, AppDbContext db, UserService userService) =>
            {
                var uid = ctx.Request.Cookies["uid"];
                var current = await db.Users.FirstOrDefaultAsync(u => u.Id == uid);

                if (current == null || current.Role != "Admin")
                    return Results.Forbid();

                var target = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (target == null) return Results.NotFound();

                // Şifre güncelle
                if (!string.IsNullOrWhiteSpace(input.Password))
                    target.PasswordHash = userService.HashPassword(input.Password);

                // Rol güncelle
                if (!string.IsNullOrWhiteSpace(input.Role))
                    target.Role = input.Role;

                // Status güncelle
                if (input.Status.HasValue)
                    target.Status = input.Status.Value;

                await db.SaveChangesAsync();
                return Results.Ok(target);
            });

            // GET user by id (admin)
            app.MapGet("/api/admin/users/{id}", async (string id, HttpContext ctx, AppDbContext db) =>
            {
                var uid = ctx.Request.Cookies["uid"];
                var current = await db.Users.FirstOrDefaultAsync(u => u.Id == uid);

                if (current == null || current.Role != "Admin")
                    return Results.Forbid();

                var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (u == null) return Results.NotFound();

                return Results.Ok(new { u.Id, u.Username, u.Role, u.Status });
            });
        }
    }
}
