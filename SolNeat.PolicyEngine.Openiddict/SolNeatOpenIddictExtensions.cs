using Microsoft.Extensions.DependencyInjection;
using SolNeat.PolicyEngine.OpenIddict.Internal;
using static global::OpenIddict.Server.OpenIddictServerEvents;

namespace SolNeat.PolicyEngine.OpenIddict;

public static class SolNeatOpenIddictExtensions
{
    public static PolicyEngineOpenIddictBuilder AddSolNeatPolicyEngine(this OpenIddictServerBuilder builder)
    {
        if (builder.Services.Any(d => d.ServiceType == typeof(IPolicyDecisionPoint)))
        {
            throw new InvalidOperationException("Security Alert: IPolicyDecisionPoint is already registered. Possible component hijacking detected.");
        }

        builder.Services.AddSingleton<PolicyEnforcementHandler>();

        builder.AddEventHandler<ProcessSignInContext>(eventBuilder =>
            eventBuilder.UseSingletonHandler<PolicyEnforcementHandler>());

        return new PolicyEngineOpenIddictBuilder(builder.Services);
    }
}