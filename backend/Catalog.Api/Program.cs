using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<CatalogDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwt["Issuer"],
            ValidateAudience = true, ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "catalog" }));

// Public APIs

// ✅ Danh sách sản phẩm → dùng ProductListItemDto
app.MapGet("/api/catalog/products",
    async (CatalogDbContext db) =>
        await db.Products
            .Select(p => new ProductListItemDto(p.Id, p.Name, p.Brand, p.Price))
            .ToListAsync())
    .WithTags("Products")
    .Produces<List<ProductListItemDto>>(StatusCodes.Status200OK);

// ✅ Chi tiết sản phẩm → dùng ProductDetailDto
app.MapGet("/api/catalog/products/{id:int}",
    async (int id, CatalogDbContext db) =>
        await db.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDetailDto(p.Id, p.Name, p.Brand, p.Price))
            .FirstOrDefaultAsync() is { } dto
                ? Results.Ok(dto)
                : Results.NotFound())
    .WithTags("Products")
    .Produces<ProductDetailDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
// Admin-only (test)
app.MapGet("/api/catalog/admin/ping", () => Results.Ok(new { ok = true }))
   .RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();
