using Microsoft.Extensions.DependencyInjection;

namespace SolNeat.PolicyEngine;

public class PolicyEngineBuilder
{
    public IServiceCollection Services { get; private set; }

    internal PolicyEngineBuilder(IServiceCollection services)
    {
        Services = services;
    }
}

public static class PolicyEngineBuilderExtensions
{
    public static PolicyEngineBuilder AddSolNeatPolicyEngine(this IServiceCollection services)
    {
        services.AddScoped<IPolicyManager, PolicyManager>();

        services.AddScoped<IPolicyDecisionPoint, PolicyDecisionPoint>();

        return new PolicyEngineBuilder(services);
    }
}
