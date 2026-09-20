using global::OpenIddict.Server;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using static global::OpenIddict.Server.OpenIddictServerEvents;

namespace SolNeat.PolicyEngine.OpenIddict.Internal;

internal sealed class PolicyEnforcementHandler : IOpenIddictServerHandler<ProcessSignInContext>
{
    private readonly IServiceProvider _serviceProvider;
    public PolicyEnforcementHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async ValueTask HandleAsync(ProcessSignInContext context)
    {
        if (context.Principal == null) return;

        using var scope = _serviceProvider.CreateScope();

        var pdp = scope.ServiceProvider.GetRequiredService<IPolicyDecisionPoint>();

        var permissions = await pdp.EvaluateAsync(context.Principal);

        foreach (var permission in permissions)
        {
            context.Principal.SetClaim("x-access-permission", permission);
        }
    }
}