using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentityCore<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DbContext>()
    .AddDefaultTokenProviders();

var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "identity" }));

app.MapPost("/api/auth/register", async (RegisterRequest req, UserManager<AppUser> userMgr, RoleManager<IdentityRole> roleMgr) =>
{
    if (!await roleMgr.RoleExistsAsync("User")) await roleMgr.CreateAsync(new IdentityRole("User"));
    var user = new AppUser { UserName = req.Email, Email = req.Email, FullName = req.FullName };
    var res = await userMgr.CreateAsync(user, req.Password);
    if (!res.Succeeded) return Results.BadRequest(res.Errors);
    await userMgr.AddToRoleAsync(user, "User");
    return Results.Ok(new { ok = true });
}).WithTags("Auth");

app.MapPost("/api/auth/login", async (LoginRequest req, UserManager<AppUser> userMgr, RoleManager<IdentityRole> roleMgr, IConfiguration cfg, DbContext db) =>
{
    var user = await userMgr.FindByEmailAsync(req.Email);
    if (user is null || !await userMgr.CheckPasswordAsync(user, req.Password))
        return Results.Unauthorized();

    var roles = await userMgr.GetRolesAsync(user);
    var role = roles.FirstOrDefault() ?? "User";

    var access = AuthHelpers.CreateJwt(user, role, cfg);
    var refresh = AuthHelpers.CreateRefreshToken(user.Id);

    db.RefreshTokens.Add(refresh);
    await db.SaveChangesAsync();

    return Results.Ok(new AuthResponse(access, refresh.Token, role, user.Email!));
}).WithTags("Auth")
  .Produces<AuthResponse>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status401Unauthorized);

app.MapPost("/api/auth/refresh", async (RefreshRequest req, UserManager<AppUser> userMgr, IConfiguration cfg, DbContext db) =>
{
    var rt = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == req.RefreshToken);
    if (rt is null || !rt.IsActive) return Results.Unauthorized();

    // rotate: revoke current, create new
    rt.RevokedAt = DateTime.UtcNow;
    var newRt = AuthHelpers.CreateRefreshToken(rt.UserId);
    rt.ReplacedByToken = newRt.Token;
    db.RefreshTokens.Add(newRt);

    var user = await userMgr.FindByIdAsync(rt.UserId);
    if (user is null) return Results.Unauthorized();

    var roles = await userMgr.GetRolesAsync(user);
    var role = roles.FirstOrDefault() ?? "User";
    var access = AuthHelpers.CreateJwt(user, role, cfg);
    await db.SaveChangesAsync();

    return Results.Ok(new AuthResponse(access, newRt.Token, role, user.Email!));
}).WithTags("Auth")
  .Produces<AuthResponse>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status401Unauthorized);

app.MapPost("/api/auth/logout", async (LogoutRequest req, DbContext db) =>
{
    var rt = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == req.RefreshToken);
    if (rt is not null && rt.IsActive) { rt.RevokedAt = DateTime.UtcNow; await db.SaveChangesAsync(); }
    return Results.Ok(new { ok = true });
}).WithTags("Auth");


app.MapPost("/api/auth/seed-admin", async (UserManager<AppUser> userMgr, RoleManager<IdentityRole> roleMgr) =>
{
    if (!await roleMgr.RoleExistsAsync("Admin")) await roleMgr.CreateAsync(new IdentityRole("Admin"));
    if (!await roleMgr.RoleExistsAsync("User")) await roleMgr.CreateAsync(new IdentityRole("User"));

    var email = "admin@local";
    var exists = await userMgr.FindByEmailAsync(email);
    if (exists is null)
    {
        var u = new AppUser { UserName = email, Email = email, FullName = "Shop Admin" };
        var r = await userMgr.CreateAsync(u, "Admin123$");
        if (r.Succeeded) await userMgr.AddToRoleAsync(u, "Admin");
    }
    return Results.Ok(new { ok = true });
}).RequireAuthorization(policy => policy.RequireRole("Admin"));
// .WithTags("Dev").AllowAnonymous();gọi 1 lần để seed admin xong có thể đổi sang RequireAuthorization()

app.Run();
