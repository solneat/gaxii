using Microsoft.Extensions.DependencyInjection;

namespace SolNeat.PolicyEngine;

public abstract class PolicyEngineBuilder
{
    protected PolicyEngineBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}
