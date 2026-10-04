using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SolNeat.PolicyEngine.EF;

public static class PolicyStoreEntityFrameworkExtensions
{
    public static PolicyEngineBuilder UseDbContext<TContext>(
        this PolicyEngineBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        builder.Services.AddDbContext<TContext>(optionsAction);

        builder.Services.AddScoped<IPolicyStore, PolicyStore<TContext>>();

        return builder;
    }

}