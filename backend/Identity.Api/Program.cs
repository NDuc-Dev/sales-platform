using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "identity" }));

// tạm thời mock endpoints để FE/Gateway test luồng
app.MapPost("/api/auth/register", () => Results.Ok(new { ok = true })).WithTags("Auth");
app.MapPost("/api/auth/login", () => Results.Ok(new { token = "fake-jwt", role = "User" })).WithTags("Auth");

app.Run();

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) {}
}
