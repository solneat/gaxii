namespace SolNeat.PolicyEngine;

public class ActorTotpSecret
{
    /// <summary>
    /// Gets or sets the foreign key and primary key matching the associated Actor.Id.
    /// </summary>
    public Guid ActorId { get; set; }

    /// <summary>
    /// Gets or sets the Base32-encoded TOTP shared secret.
    /// </summary>
    public string Secret { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the user has verified initial QR enrollment.
    /// </summary>
    public bool IsConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the secret was enrolled.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when the secret was last used for verification.
    /// </summary>
    public DateTime? LastUsedAtUtc { get; set; }
}