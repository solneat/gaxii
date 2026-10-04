namespace SolNeat.PolicyEngine;

public sealed class ActorPasskey
{
    /// <summary>
    /// Gets or sets the unique internal identifier of the Passkey record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the identifier of the associated Actor.
    /// </summary>
    public Guid ActorId { get; set; }

    /// <summary>
    /// Gets or sets the WebAuthn Credential ID provided by the authenticator.
    /// </summary>
    public byte[] CredentialId { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the public key encoded in COSE structure.
    /// </summary>
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the user handle bytes bound during enrollment.
    /// </summary>
    public byte[] UserHandle { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the signature counter used for replay protection.
    /// </summary>
    public uint SignatureCounter { get; set; }

    /// <summary>
    /// Gets or sets the Authenticator Attestation GUID (AAGUID).
    /// </summary>
    public Guid Aaguid { get; set; }

    /// <summary>
    /// Gets or sets the human-readable device identifier (e.g., "YubiKey 5C", "MacBook TouchID").
    /// </summary>
    public string DeviceName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UTC timestamp when the Passkey was registered.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when the Passkey was last used for authentication.
    /// </summary>
    public DateTime? LastUsedAtUtc { get; set; }
}
