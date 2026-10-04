namespace SolNeat.PolicyEngine;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid SolutionId { get; set; }
    public Solution Solution { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public ICollection<ActorRole> ActorRoles { get; set; } = new List<ActorRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
