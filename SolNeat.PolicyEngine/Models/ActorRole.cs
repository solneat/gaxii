namespace SolNeat.PolicyEngine;

public class ActorRole
{
    public Guid ActorId { get; set; }
    public Actor Actor { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

