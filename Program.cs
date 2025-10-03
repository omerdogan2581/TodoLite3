using Microsoft.EntityFrameworkCore;
using TodoLite.Data;
using TodoLite.Endpoints;
using TodoLite.Services;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core DbContext ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// --- Servisler ---
builder.Services.AddScoped<UserService>();
// Eğer TodoService EFCore’a göre güncellediysen:
builder.Services.AddScoped<TodoService>();

// --- API özellikleri ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
));

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();

// Default yönlendirme
app.MapGet("/", (HttpContext ctx) =>
{
    if (ctx.Request.Cookies.TryGetValue("uid", out var uid) && !string.IsNullOrEmpty(uid))
        ctx.Response.Redirect("/index.html");
    else
        ctx.Response.Redirect("/login.html");

    return Results.Empty;
});

// Endpoint grupları
app.MapAuthEndpoints();
app.MapTodoEndpoints();
app.MapAdminEndpoints();

app.Run();
