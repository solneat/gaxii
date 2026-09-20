using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SolNeat.PolicyEngine;
using SolNeat.PolicyEngine.EF;

namespace SolNeat.AuthDemo;

public class DbInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DbInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        // Создаем базы данных для теста
        //var policyDb = scope.ServiceProvider.GetRequiredService<SolNeatPolicyEngineDbContext>();
        //await policyDb.Database.EnsureCreatedAsync(cancellationToken);

        var oidcDb = scope.ServiceProvider.GetRequiredService<OpenIddictContext>();
        await oidcDb.Database.EnsureCreatedAsync(cancellationToken);

        var m = scope.ServiceProvider.GetRequiredService<IPolicyManager>();
        var actor = await m.GetOrCreateActorAsync("postman-client", cancellationToken);
        var role = await m.CreateRoleAsync("worker", cancellationToken);
        var permision = await m.CreatePermissionAsync("api", "post", cancellationToken);
        await m.AssignPermissionToRoleAsync(role.Id, permision.Id, cancellationToken);
        await m.AssignRoleToActorAsync(actor.Id, role.Id);

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("postman-client", cancellationToken) == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "postman-client",
                ClientSecret = "secret_key",
                DisplayName = "Postman / HTTP Client",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                }
            }, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}