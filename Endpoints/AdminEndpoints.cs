using System.Text.Json;
using TodoLite.Models;
using TodoLite.Services;

namespace TodoLite.Endpoints
{
    public static class AdminEndpoints
    {
        public static void MapAdminEndpoints(this WebApplication app)
        {
            // Kullanıcıları listele
            app.MapGet("/api/admin/users", async (HttpContext ctx, UserService userService) =>
            {
                var uid = ctx.Request.Cookies["uid"];
                var users = await userService.LoadUsers();
                var current = users.FirstOrDefault(u => u.Id == uid);
                if (current == null || current.Role != "Admin")
                    return Results.Forbid();

                return Results.Ok(users);
            });

            // Kullanıcı güncelleme endpoint
            app.MapPut("/api/admin/users/{id}", async (string id, UserUpdateDto input, HttpContext ctx, UserService userService) =>
            {
                var uid = ctx.Request.Cookies["uid"];
                var users = await userService.LoadUsers();
                var current = users.FirstOrDefault(u => u.Id == uid);
                if (current == null || current.Role != "Admin")
                    return Results.Forbid();

                var target = users.FirstOrDefault(x => x.Id == id);
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

                await userService.SaveUsers(users);
                return Results.Ok(target);
            });

            // GET user by id (admin)
            app.MapGet("/api/admin/users/{id}", async (string id, UserService userService) =>
            {
                var users = await userService.LoadUsers();
                var u = users.FirstOrDefault(x => x.Id == id);
                if (u == null) return Results.NotFound();
                return Results.Ok(new { u.Id, u.Username, u.Role, u.Status });
            });


        }
    }
}
