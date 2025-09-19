using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class DbContext : IdentityDbContext<AppUser>
{
    public DbContext(DbContextOptions<DbContext> o) : base(o) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}
