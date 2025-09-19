using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;

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

// serve static files (wwwroot)
var www = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(Path.Combine(www, "images", "products"));
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(www),
    RequestPath = ""
});

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "catalog" }));

// Public APIs
// Admin: create
app.MapPost("/api/catalog/admin/products", async (ProductCreateDto body, CatalogDbContext db) =>
{
    var p = new Product { Name = body.Name, Brand = body.Brand, Price = body.Price, Description = body.Description };
    db.Add(p); await db.SaveChangesAsync();
    return Results.Created($"/api/catalog/products/{p.Id}", new { p.Id });
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithTags("Admin")
.Produces(StatusCodes.Status201Created);

// Admin: update
app.MapPut("/api/catalog/admin/products/{id:int}", async (int id, ProductUpdateDto body, CatalogDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    p.Name = body.Name; p.Brand = body.Brand; p.Price = body.Price; p.Description = body.Description;
    await db.SaveChangesAsync();
    return Results.Ok(new { ok = true });
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithTags("Admin");

// Admin: delete
app.MapDelete("/api/catalog/admin/products/{id:int}", async (int id, CatalogDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Remove(p); await db.SaveChangesAsync();
    return Results.Ok(new { ok = true });
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithTags("Admin");

// Admin: upload image (multipart/form-data)
app.MapPost("/api/catalog/admin/products/{id:int}/image", async (int id, HttpRequest req, CatalogDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();

    if (!req.HasFormContentType) return Results.BadRequest(new { error = "FormData required" });
    var file = req.Form.Files["file"];
    if (file is null || file.Length == 0) return Results.BadRequest(new { error = "file is required" });

    var ext = Path.GetExtension(file.FileName);
    var fname = $"prod_{id}_{Guid.NewGuid():N}{ext}";
    var path = Path.Combine(www, "images", "products", fname);
    using (var fs = File.Create(path)) { await file.CopyToAsync(fs); }

    p.ImageUrl = $"/images/products/{fname}";
    await db.SaveChangesAsync();
    return Results.Ok(new { p.ImageUrl });
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithTags("Admin")
.Accepts<IFormFile>("multipart/form-data")
.Produces(StatusCodes.Status200OK);

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
