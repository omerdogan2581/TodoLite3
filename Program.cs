
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TodoLite.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
));
var app = builder.Build();

app.UseCors();
app.UseStaticFiles();

app.MapGet("/", (HttpContext ctx) =>
{
    // Kullanıcı giriş yapmışsa index.html'e yönlendir
    if (ctx.Request.Cookies.TryGetValue("uid", out var uid) && !string.IsNullOrEmpty(uid))
    {
        ctx.Response.Redirect("/index.html");
    }
    else
    {
        ctx.Response.Redirect("/login.html");
    }

    return Results.Empty;
});


var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDir);

string userFile = Path.Combine(dataDir, "users.json");
string todoFile = Path.Combine(dataDir, "todos.json");

// ---------------- Helper ----------------
static string HashPassword(string plain)
{
    using var sha = SHA256.Create();
    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
}

async Task<List<User>> LoadUsers() =>
    File.Exists(userFile)
        ? JsonSerializer.Deserialize<List<User>>(await File.ReadAllTextAsync(userFile)) ?? new()
        : new();

async Task SaveUsers(List<User> list) =>
    await File.WriteAllTextAsync(userFile,
        JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));

async Task<List<TodoItem>> LoadTodos() =>
    File.Exists(todoFile)
        ? JsonSerializer.Deserialize<List<TodoItem>>(await File.ReadAllTextAsync(todoFile)) ?? new()
        : new();

async Task SaveTodos(List<TodoItem> list) =>
    await File.WriteAllTextAsync(todoFile,
        JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));



// ---------------- Auth Middleware ----------------
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

// ---------------- Auth Endpoints ----------------

// REGISTER → Yeni kullanıcı
app.MapPost("/api/auth/register", async (HttpContext ctx) =>
{
    var input = await JsonSerializer.DeserializeAsync<User>(ctx.Request.Body);
    if (input == null || string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.PasswordHash))
        return Results.BadRequest("Geçersiz kullanıcı verisi.");

    var users = await LoadUsers();
    if (users.Any(u => u.Username.Equals(input.Username, StringComparison.OrdinalIgnoreCase)))
        return Results.Conflict("Bu kullanıcı adı zaten alınmış.");

    var newUser = new User
    {
        Id = Guid.NewGuid().ToString(),
        Username = input.Username,
        PasswordHash = HashPassword(input.PasswordHash)
    };

    users.Add(newUser);
    await SaveUsers(users);
    return Results.Ok(new { newUser.Id, newUser.Username });
});

// LOGIN
app.MapPost("/api/auth/login", async (HttpContext ctx) =>
{
    var input = await JsonSerializer.DeserializeAsync<User>(ctx.Request.Body);
    if (input == null) return Results.BadRequest();

    var users = await LoadUsers();
    var u = users.FirstOrDefault(x =>
        x.Username == input.Username &&
        x.PasswordHash == HashPassword(input.PasswordHash)
    );

    if (u == null) return Results.Unauthorized();

    ctx.Response.Cookies.Append("uid", u.Id,
        new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax });

    return Results.Ok(new { u.Id, u.Username });
});

// LOGOUT
app.MapPost("/api/auth/logout", (HttpContext ctx) =>
{
    ctx.Response.Cookies.Delete("uid");
    return Results.Ok();
});

// ---------------- Todo Endpoints ----------------

// GET → Sadece kendi todo’ları
app.MapGet("/api/todos", async (HttpContext ctx) =>
{
    var uid = ctx.Items["uid"]?.ToString();
    var list = await LoadTodos();
    return list.Where(t => t.CreatorUserId == uid);
});

// POST → Yeni todo
app.MapPost("/api/todos", async (HttpContext ctx) =>
{
    var uid = ctx.Items["uid"]?.ToString();
    var t = await JsonSerializer.DeserializeAsync<TodoItem>(ctx.Request.Body);
    if (t == null) return Results.BadRequest();

    t.Id = Guid.NewGuid();
    t.CreatorUserId = uid;
    t.CreatedAt = DateTime.UtcNow;

    var list = await LoadTodos();
    list.Add(t);
    await SaveTodos(list);

    return Results.Ok(t);
});

// PUT → Güncelle
app.MapPut("/api/todos/{id:guid}", async (HttpContext ctx, Guid id) =>
{
    var uid = ctx.Items["uid"]?.ToString();
    var input = await JsonSerializer.DeserializeAsync<TodoItem>(ctx.Request.Body);
    if (input == null) return Results.BadRequest();

    var list = await LoadTodos();
    var t = list.FirstOrDefault(x => x.Id == id && x.CreatorUserId == uid);
    if (t == null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(input.Text)) t.Text = input.Text;
    if (!string.IsNullOrWhiteSpace(input.Status)) t.Status = input.Status;

    await SaveTodos(list);
    return Results.Ok(t);
});

// DELETE → Sil
app.MapDelete("/api/todos/{id:guid}", async (HttpContext ctx, Guid id) =>
{
    var uid = ctx.Items["uid"]?.ToString();
    var list = await LoadTodos();
    var t = list.FirstOrDefault(x => x.Id == id && x.CreatorUserId == uid);
    if (t == null) return Results.NotFound();

    list.Remove(t);
    await SaveTodos(list);
    return Results.Ok();
});

app.Run();
