using Microsoft.EntityFrameworkCore;
using SolNeat.PolicyEngine.EF;

namespace SolNeat.AuthDemo;

public class OpenIddictContext : DbContext
{
    public OpenIddictContext(DbContextOptions<OpenIddictContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseOpenIddict();

        modelBuilder.UseSolNeatPolicyEngine();
    }
}