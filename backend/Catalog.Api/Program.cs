using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CatalogDb>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "catalog" }));

app.MapGet("/api/catalog/products", async (CatalogDb db) =>
    await db.Products.Select(p => new { p.Id, p.Name, p.Brand, p.Price }).ToListAsync());

app.MapGet("/api/catalog/products/{id:int}", async (int id, CatalogDb db) =>
    await db.Products.FirstOrDefaultAsync(p => p.Id == id) is { } p
        ? Results.Ok(p)
        : Results.NotFound());

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDb>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { Name = "Basic Tee", Brand = "NUT", Price = 199000 },
            new Product { Name = "Slim Jeans", Brand = "NUT", Price = 499000 },
            new Product { Name = "Sneaker White", Brand = "NUT", Price = 899000 }
        );
        await db.SaveChangesAsync();
    }
}

app.Run();

public class CatalogDb : DbContext
{
    public CatalogDb(DbContextOptions<CatalogDb> o) : base(o) {}
    public DbSet<Product> Products => Set<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public int Price { get; set; } // VND
}
