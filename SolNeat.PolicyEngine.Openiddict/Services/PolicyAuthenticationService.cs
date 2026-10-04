namespace SolNeat.PolicyEngine.OpenIddict;

public interface IPolicyAuthenticationService
{
    Task<Actor?> AuthenticatePasswordAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task<Actor?> GetActorByIdAsync(
        Guid actorId,
        CancellationToken cancellationToken = default);

    Task<ActorTotpSecret?> GetTotpSecretAsync(
        Guid actorId,
        CancellationToken cancellationToken = default);

    Task<ActorTotpSecret> CreateTotpSecretAsync(
        Guid actorId,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyTotpAsync(
        Guid actorId,
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyAndConfirmTotpAsync(
        Guid actorId,
        string code,
        CancellationToken cancellationToken = default);

    Task ConfirmTotpAsync(
        Guid actorId,
        CancellationToken cancellationToken = default);
}

internal sealed class PolicyAuthenticationService : IPolicyAuthenticationService
{
    private readonly ILdapAuthenticator _ldapAuthenticator;
    private readonly ITotpService _totpService;
    private readonly IPolicyStore _policyStore;

    public PolicyAuthenticationService(
        ILdapAuthenticator ldapAuthenticator,
        ITotpService totpService,
        IPolicyStore policyStore)
    {
        _ldapAuthenticator = ldapAuthenticator;
        _totpService = totpService;
        _policyStore = policyStore;
    }

    public Task<Actor?> GetActorByIdAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        return _policyStore.FindActorByIdAsync(actorId, cancellationToken);
    }

    public async Task<Actor?> AuthenticatePasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        return await _ldapAuthenticator.AuthenticateAsync(username, password, cancellationToken).ConfigureAwait(false);
    }

    public async Task ConfirmTotpAsync(
          Guid actorId,
          CancellationToken cancellationToken = default)
    {
        var totpSecret = await _policyStore
            .GetTotpSecretAsync(
                actorId,
                cancellationToken)
            .ConfigureAwait(false);

        if (totpSecret == null)
        {
            throw new InvalidOperationException(
                $"TOTP secret does not exist for actor '{actorId}'.");
        }

        if (totpSecret.IsConfirmed)
            return;

        await _policyStore
            .ConfirmTotpSecretAsync(
                actorId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<ActorTotpSecret> CreateTotpSecretAsync(
            Guid actorId,
            CancellationToken cancellationToken = default)
    {
        // Don't overwrite an existing secret.
        var existing = await _policyStore
            .GetTotpSecretAsync(
                actorId,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing != null)
        {
            return existing;
        }

        var secret = _totpService.GenerateSecret();

        await _policyStore
            .SetTotpSecretAsync(
                actorId,
                secret,
                isConfirmed: false,
                cancellationToken)
            .ConfigureAwait(false);

        var created = await _policyStore
            .GetTotpSecretAsync(
                actorId,
                cancellationToken)
            .ConfigureAwait(false);

        if (created == null)
        {
            throw new InvalidOperationException(
                $"TOTP secret was not created for actor '{actorId}'.");
        }

        return created;
    }

    public async Task<ActorTotpSecret?> GetTotpSecretAsync(
           Guid actorId,
           CancellationToken cancellationToken = default)
    {
        return await _policyStore
            .GetTotpSecretAsync(
                actorId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> VerifyAndConfirmTotpAsync(
        Guid actorId,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var totpSecret = await _policyStore
            .GetTotpSecretAsync(
                actorId,
                cancellationToken)
            .ConfigureAwait(false);

        if (totpSecret == null)
            return false;

        var isValid = _totpService.ValidateCode(
            totpSecret.Secret,
            code);

        if (!isValid)
            return false;

        await _policyStore
            .ConfirmTotpSecretAsync(
                actorId,
                cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    public async Task<bool> VerifyTotpAsync(Guid actorId, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var totpSecret = await _policyStore.GetTotpSecretAsync(actorId, cancellationToken).ConfigureAwait(false);
        if (totpSecret == null || !totpSecret.IsConfirmed)
            return false;

        return _totpService.ValidateCode(totpSecret.Secret, code);
    }
}