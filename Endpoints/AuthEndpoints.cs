using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TodoLite.Data;
using TodoLite.Models;
using TodoLite.Services;

namespace TodoLite.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            // REGISTER
            app.MapPost("/api/auth/register", async (HttpContext ctx, AppDbContext db, UserService userService) =>
            {
                var input = await JsonSerializer.DeserializeAsync<User>(ctx.Request.Body);
                if (input == null || string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.PasswordHash))
                    return Results.BadRequest("Geçersiz kullanıcı verisi.");

                var exists = await db.Users.AnyAsync(u => u.Username.ToLower() == input.Username.ToLower());
                if (exists)
                    return Results.Conflict("Bu kullanıcı adı zaten alınmış.");

                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = input.Username,
                    PasswordHash = userService.HashPassword(input.PasswordHash),
                    Status = false, // admin onayı lazım
                    Role = "User"
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();

                return Results.Ok(new { newUser.Id, newUser.Username });
            });

            // LOGIN
            app.MapPost("/api/auth/login", async (HttpContext ctx, AppDbContext db, UserService userService) =>
            {
                var input = await JsonSerializer.DeserializeAsync<User>(ctx.Request.Body);
                if (input == null) return Results.BadRequest();

                var hash = userService.HashPassword(input.PasswordHash);

                var u = await db.Users
                    .FirstOrDefaultAsync(x => x.Username == input.Username && x.PasswordHash == hash);

                if (u == null) return Results.Unauthorized();
                if (!u.Status) return Results.Forbid();

                ctx.Response.Cookies.Append("uid", u.Id,
                    new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax });

                return Results.Ok(new { u.Id, u.Username, u.Role });
            });

            // LOGOUT
            app.MapPost("/api/auth/logout", (HttpContext ctx) =>
            {
                ctx.Response.Cookies.Delete("uid");
                return Results.Ok();
            });

            // ME (aktif kullanıcı bilgisi)
            app.MapGet("/api/auth/me", async (HttpContext ctx, AppDbContext db) =>
            {
                var uid = ctx.Request.Cookies["uid"];
                if (string.IsNullOrEmpty(uid))
                    return Results.Unauthorized();

                var u = await db.Users.FirstOrDefaultAsync(x => x.Id == uid);
                if (u == null)
                    return Results.Unauthorized();

                return Results.Ok(new { u.Id, u.Username, u.Role, u.Status });
            });
        }
    }
}
