using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using static global::OpenIddict.Server.OpenIddictServerEvents;
using static OpenIddict.Server.OpenIddictServerHandlers;

namespace SolNeat.PolicyEngine.OpenIddict;

public static class PolicyEngineOpenIddictBuilderExtensions
{
    public static PolicyEngineOpenIddictBuilder AddSolNeatPolicyEngine(this OpenIddictServerBuilder builder)
    {
        builder.Services.AddScoped<IPolicyManager, PolicyManager>();

        builder.Services.AddScoped<IPolicyDecisionPoint, PolicyDecisionPoint>();

        builder.Services.AddSingleton<PolicyEnforcementHandler>();

        builder.AddEventHandler<ProcessSignInContext>(eventBuilder => eventBuilder
            .UseSingletonHandler<PolicyEnforcementHandler>()
            .SetOrder(Protection.GenerateIdentityModelToken.Descriptor.Order - 1));

        return new PolicyEngineOpenIddictBuilder(builder);
    }
}