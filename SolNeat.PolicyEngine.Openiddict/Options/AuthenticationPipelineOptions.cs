namespace SolNeat.PolicyEngine.OpenIddict;

public sealed class AuthenticationPipelineOptions
{
    /// <summary>
    /// Gets or sets the issuer string embedded in TOTP QR codes.
    /// </summary>
    public string TotpIssuer { get; set; } = "SolNeat";

    /// <summary>
    /// Gets or sets the WebAuthn Relying Party Name for Passkey authentication.
    /// </summary>
    public string PasskeyRelyingPartyName { get; set; } = "SolNeat Engine";

    /// <summary>
    /// Gets or sets the WebAuthn Relying Party ID (domain name, e.g., "auth.solneat.com").
    /// </summary>
    public string PasskeyRelyingPartyId { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets LDAP Directory configuration settings.
    /// </summary>
    public LdapOptions Ldap { get; set; } = new();

    public bool EnableTotpMfa { get; set; } = false;
    public bool EnablePasskeyMfa { get; set; } = false;
}