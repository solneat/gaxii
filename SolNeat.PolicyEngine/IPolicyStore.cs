namespace SolNeat.PolicyEngine;

public interface IPolicyStore
{
    Task<Actor?> FindActorBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetPermissionsForActorAsync(string subjectId, CancellationToken cancellationToken = default);

    Task<Actor> GetOrCreateActorAsync(string subjectId, CancellationToken cancellationToken = default);
    Task<Permission> CreatePermissionAsync(string resource, string action, CancellationToken cancellationToken = default);
    Task<Role> CreateRoleAsync(string name, CancellationToken cancellationToken = default);
    Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task AssignRoleToActorAsync(Guid actorId, Guid roleId, CancellationToken cancellationToken = default);
}