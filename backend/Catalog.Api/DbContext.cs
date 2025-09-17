using Microsoft.EntityFrameworkCore;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> o) : base(o) { }
    public DbSet<Product> Products => Set<Product>();
}
