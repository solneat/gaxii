using Microsoft.Extensions.DependencyInjection;
using SolNeat.PolicyEngine.OpenIddict.Internal;
using static global::OpenIddict.Server.OpenIddictServerEvents;

namespace SolNeat.PolicyEngine.OpenIddict;

public class PolicyEngineOpenIddictBuilder
{
    internal PolicyEngineOpenIddictBuilder(IServiceCollection services)
    {
    }
}

public static class PolicyEngineOpenIddictBuilderExtensions
{
    public static PolicyEngineOpenIddictBuilder UseOpenIddict(this PolicyEngineBuilder builder, OpenIddictServerBuilder openIddict)
    {
        builder.Services.AddSingleton<PolicyEnforcementHandler>();

        openIddict.AddEventHandler<ProcessSignInContext>(eventBuilder =>
            eventBuilder.UseSingletonHandler<PolicyEnforcementHandler>());

        return new PolicyEngineOpenIddictBuilder(builder.Services);
    }
}