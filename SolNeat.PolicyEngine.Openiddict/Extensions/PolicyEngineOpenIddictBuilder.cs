using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace SolNeat.PolicyEngine.OpenIddict;

public class PolicyEngineOpenIddictBuilder : PolicyEngineBuilder
{
    private readonly OpenIddictServerBuilder _openIddictServer;
    
    internal PolicyEngineOpenIddictBuilder(OpenIddictServerBuilder openIddictServer) : base(openIddictServer.Services) 
    { 
        _openIddictServer = openIddictServer;
    }

    public PolicyEngineOpenIddictBuilder ConfigureAuthentication(Action<AuthenticationPipelineOptions>? configure = null)
    {
        configure ??= (c) => new AuthenticationPipelineOptions();
        
        Services.Configure(configure);

        Services.TryAddTransient<ILdapAuthenticator, LdapAuthenticator>();
        Services.TryAddTransient<ITotpService, TotpService>();
        Services.TryAddTransient<IPasskeyService, PasskeyService>();
        Services.TryAddTransient<IPolicyAuthenticationService, PolicyAuthenticationService>();

        return this;
    }

    public PolicyEngineOpenIddictBuilder UseLoginEngine(Action<LoginEngineOptions>? configure = null)
    {
        configure ??= (c) => new LoginEngineOptions();

        Services.Configure(configure);

        Services.AddDataProtection();

        Services.AddTransient<IStartupFilter, PolicyLoginEngineMiddlewareStartupFilter>();

        return this;
    }
}
