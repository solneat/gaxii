using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SolNeat.PolicyEngine.EF;

public static class EfPolicyStoreExtensions
{

    public static PolicyEngineBuilder UseEntityFrameworkCore(
        this PolicyEngine.PolicyEngineBuilder builder)
    {
        return new PolicyEngineBuilder(builder.Services);
    }

    public static PolicyEngineBuilder UseDbContext<TContext>(
        this PolicyEngineBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        if (builder.Services.Any(d => d.ServiceType == typeof(IPolicyStore)))
        {
            throw new InvalidOperationException("Security Alert: IPolicyStore is already registered. Duplicate or malicious policy store override detected.");
        }

        builder.Services.AddDbContext<TContext>(optionsAction);
        builder.Services.AddScoped<IPolicyStore, EfPolicyStore<TContext>>();

        return builder;
    }

}