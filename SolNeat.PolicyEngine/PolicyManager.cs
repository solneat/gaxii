namespace SolNeat.PolicyEngine;

public interface IPolicyManager
{
    Task<Actor> GetOrCreateActorAsync(string subjectId, CancellationToken cancellationToken = default);
    Task<Permission> CreatePermissionAsync(string resource, string action, CancellationToken cancellationToken = default);
    Task<Role> CreateRoleAsync(string name, CancellationToken cancellationToken = default);
    Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task AssignRoleToActorAsync(Guid actorId, Guid roleId, CancellationToken cancellationToken = default);
}

public sealed class PolicyManager : IPolicyManager
{
    private readonly IPolicyStore _store;

    public PolicyManager(IPolicyStore store) => _store = store;

    public Task<Actor> GetOrCreateActorAsync(string subjectId, CancellationToken cancellationToken = default)
        => _store.GetOrCreateActorAsync(subjectId, cancellationToken);

    public Task<Permission> CreatePermissionAsync(string resource, string action, CancellationToken cancellationToken = default)
        => _store.CreatePermissionAsync(resource, action, cancellationToken);

    public Task<Role> CreateRoleAsync(string name, CancellationToken cancellationToken = default)
        => _store.CreateRoleAsync(name, cancellationToken);

    public Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        => _store.AssignPermissionToRoleAsync(roleId, permissionId, cancellationToken);

    public Task AssignRoleToActorAsync(Guid actorId, Guid roleId, CancellationToken cancellationToken = default)
        => _store.AssignRoleToActorAsync(actorId, roleId, cancellationToken);
}