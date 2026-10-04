using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Options;

namespace SolNeat.PolicyEngine.OpenIddict;

public interface IPasskeyService
{
    Task<AssertionOptions> GenerateAssertionOptionsAsync(IReadOnlyCollection<ActorPasskey> existingPasskeys);
    Task<bool> VerifyAssertionAsync(
        ActorPasskey passkey, 
        AuthenticatorAssertionRawResponse rawResponse, 
        AssertionOptions originalOptions,
        CancellationToken ct);
}

internal sealed class PasskeyService : IPasskeyService
{
    private readonly Fido2 _fido2;

    public PasskeyService(IOptions<AuthenticationPipelineOptions> options)
    {
        var opt = options.Value ?? throw new ArgumentNullException(nameof(options));

        _fido2 = new Fido2(new Fido2Configuration
        {
            RPName = opt.PasskeyRelyingPartyName,
            RPID = opt.PasskeyRelyingPartyId,
            Origins = new HashSet<string> { $"https://{opt.PasskeyRelyingPartyId}" }
        });
    }

    public Task<AssertionOptions> GenerateAssertionOptionsAsync(IReadOnlyCollection<ActorPasskey> existingPasskeys)
    {
        var allowedCredentials = existingPasskeys
            .Select(p => new PublicKeyCredentialDescriptor(p.CredentialId))
            .ToList();

        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allowedCredentials,
            UserVerification = UserVerificationRequirement.Discouraged
        });

        return Task.FromResult(options);
    }

    public async Task<bool> VerifyAssertionAsync(
        ActorPasskey passkey, 
        AuthenticatorAssertionRawResponse rawResponse, 
        AssertionOptions originalOptions,
        CancellationToken ct = default)
    {
        var result = await _fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = rawResponse,
            OriginalOptions = originalOptions,
            StoredPublicKey = passkey.PublicKey,
            StoredSignatureCounter = passkey.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = async (args, ct) => true
        }, ct).ConfigureAwait(false);

        return result != null;
    }
}