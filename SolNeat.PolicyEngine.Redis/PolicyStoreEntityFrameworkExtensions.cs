using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace SolNeat.PolicyEngine.Redis;

public static class PolicyStoreRedisExtensions
{
    public static PolicyEngineBuilder UseRedis(
        this PolicyEngineBuilder builder,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(connectionString));

        builder.Services.AddScoped<IPolicyStore, RedisPolicyStore>();

        return builder;
    }

    public static PolicyEngineBuilder UseRedis(
        this PolicyEngineBuilder builder,
        IConnectionMultiplexer connectionMultiplexer)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(connectionMultiplexer);

        builder.Services.AddSingleton(connectionMultiplexer);
        builder.Services.AddScoped<IPolicyStore, RedisPolicyStore>();

        return builder;
    }
}