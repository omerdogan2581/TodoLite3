using TodoLite.Endpoints;
using TodoLite.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
));

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<TodoService>();

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();

// default yönlendirme
app.MapGet("/", (HttpContext ctx) =>
{
    if (ctx.Request.Cookies.TryGetValue("uid", out var uid) && !string.IsNullOrEmpty(uid))
        ctx.Response.Redirect("/index.html");
    else
        ctx.Response.Redirect("/login.html");

    return Results.Empty;
});

// endpointleri buradan çağırıyoruz
app.MapAuthEndpoints();
app.MapTodoEndpoints();
app.MapAdminEndpoints();

app.Run();
