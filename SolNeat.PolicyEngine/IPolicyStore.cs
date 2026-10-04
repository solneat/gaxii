namespace SolNeat.PolicyEngine;

public interface IPolicyStore
{
    Task<Actor?> FindActorBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default);
    Task<Actor?> FindActorByIdAsync(Guid actorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetPermissionsForActorAsync(string subjectId, CancellationToken cancellationToken = default);
    Task<Actor> CreateActorAsync(string subjectId, string preferredName, CancellationToken cancellationToken = default);
    Task<Actor> GetOrCreateActorAsync(string subjectId, CancellationToken cancellationToken = default);
    Task<Solution> CreateSolutionAsync(string name, string? description = null, CancellationToken cancellationToken = default);
    Task<Solution?> FindSolutionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Solution?> FindSolutionByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role> CreateRoleAsync(Guid solutionId, string name, CancellationToken cancellationToken = default);
    Task<Permission> CreatePermissionAsync(string resource, string action, CancellationToken cancellationToken = default);
    Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task AssignRoleToActorAsync(Guid actorId, Guid roleId, CancellationToken cancellationToken = default);

    // --- TOTP Operations ---
    Task SetTotpSecretAsync(Guid actorId, string secret, bool isConfirmed = false, CancellationToken cancellationToken = default);
    Task ConfirmTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default);
    Task<ActorTotpSecret?> GetTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default);
    Task RemoveTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default);

    // --- Passkey / WebAuthn Operations ---
    Task AddPasskeyAsync(ActorPasskey passkey, CancellationToken cancellationToken = default);
    Task<ActorPasskey?> GetPasskeyByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ActorPasskey>> GetPasskeysByActorIdAsync(Guid actorId, CancellationToken cancellationToken = default);
    Task UpdatePasskeyCounterAsync(Guid passkeyId, uint newCounter, CancellationToken cancellationToken = default);
    Task RemovePasskeyAsync(Guid passkeyId, CancellationToken cancellationToken = default);
}