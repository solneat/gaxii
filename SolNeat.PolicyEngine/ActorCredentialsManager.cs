namespace SolNeat.PolicyEngine;

public interface IActorCredentialsManager
{
    // --- TOTP ---
    Task SetTotpSecretAsync(Guid actorId, string secret, bool isConfirmed = false, CancellationToken cancellationToken = default);
    Task ConfirmTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default);
    Task<ActorTotpSecret?> GetTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default);
    Task RemoveTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default);

    // --- Passkeys ---
    Task AddPasskeyAsync(ActorPasskey passkey, CancellationToken cancellationToken = default);
    Task<ActorPasskey?> GetPasskeyByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ActorPasskey>> GetPasskeysByActorIdAsync(Guid actorId, CancellationToken cancellationToken = default);
    Task UpdatePasskeyCounterAsync(Guid passkeyId, uint newCounter, CancellationToken cancellationToken = default);
    Task RemovePasskeyAsync(Guid passkeyId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides domain management for Actor authentication factors (TOTP and Passkeys).
/// </summary>
public sealed class ActorCredentialsManager : IActorCredentialsManager
{
    private readonly IPolicyStore _store;

    public ActorCredentialsManager(IPolicyStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    // --- TOTP ---
    public Task SetTotpSecretAsync(Guid actorId, string secret, bool isConfirmed = false, CancellationToken cancellationToken = default)
        => _store.SetTotpSecretAsync(actorId, secret, isConfirmed, cancellationToken);

    public Task ConfirmTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
        => _store.ConfirmTotpSecretAsync(actorId, cancellationToken);

    public Task<ActorTotpSecret?> GetTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
        => _store.GetTotpSecretAsync(actorId, cancellationToken);

    public Task RemoveTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
        => _store.RemoveTotpSecretAsync(actorId, cancellationToken);

    // --- Passkey ---
    public Task AddPasskeyAsync(ActorPasskey passkey, CancellationToken cancellationToken = default)
        => _store.AddPasskeyAsync(passkey, cancellationToken);

    public Task<ActorPasskey?> GetPasskeyByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default)
        => _store.GetPasskeyByCredentialIdAsync(credentialId, cancellationToken);

    public Task<IReadOnlyCollection<ActorPasskey>> GetPasskeysByActorIdAsync(Guid actorId, CancellationToken cancellationToken = default)
        => _store.GetPasskeysByActorIdAsync(actorId, cancellationToken);

    public Task UpdatePasskeyCounterAsync(Guid passkeyId, uint newCounter, CancellationToken cancellationToken = default)
        => _store.UpdatePasskeyCounterAsync(passkeyId, newCounter, cancellationToken);

    public Task RemovePasskeyAsync(Guid passkeyId, CancellationToken cancellationToken = default)
        => _store.RemovePasskeyAsync(passkeyId, cancellationToken);
}