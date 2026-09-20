using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using SolNeat.AuthDemo;
using SolNeat.PolicyEngine;
using SolNeat.PolicyEngine.EF;
using SolNeat.PolicyEngine.OpenIddict;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OpenIddictContext>(options =>
{
    options.UseSqlite("Data Source=auth.db");
});

var peBuilder = builder.Services.AddSolNeatPolicyEngine();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<OpenIddictContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");

        options.AllowClientCredentialsFlow();

        options.AddDevelopmentEncryptionCertificate();
        options.AddDevelopmentSigningCertificate();

        options.UseAspNetCore().DisableTransportSecurityRequirement();
        peBuilder.UseEntityFrameworkCore()
            .UseDbContext<OpenIddictContext>();

    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddHostedService<DbInitializer>();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();