using global::OpenIddict.Server;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static global::OpenIddict.Server.OpenIddictServerEvents;

namespace SolNeat.PolicyEngine.OpenIddict;

internal sealed class PolicyEnforcementHandler : IOpenIddictServerHandler<ProcessSignInContext>
{
    private readonly IServiceProvider _serviceProvider;
    public PolicyEnforcementHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async ValueTask HandleAsync(ProcessSignInContext context)
    {
        if (context.Principal == null) return;

        if (context.Principal.Identity is not ClaimsIdentity identity) return;
        
         using var scope = _serviceProvider.CreateScope();

         var pdp = scope.ServiceProvider.GetRequiredService<IPolicyDecisionPoint>();

         var permissions = await pdp.EvaluateAsync(context.Principal);

        foreach (var permission in permissions)
        {
            var claim = new Claim("x-access-permission", permission);
            claim.SetDestinations(
                OpenIddictConstants.Destinations.AccessToken, 
                OpenIddictConstants.Destinations.IdentityToken);
            identity.AddClaim(claim);
        }
    }
}