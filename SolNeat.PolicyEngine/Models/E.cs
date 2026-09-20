namespace SolNeat.PolicyEngine;

public class Actor
{
    public string Solution { get; set; } = "default";
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = null!;
    public string SubjectId { get; set; } = null!; // sub или client_id из OpenIddict
    public ICollection<ActorRole> ActorRoles { get; set; } = new List<ActorRole>();
}

public class Role
{
    public string Solution { get; set; } = "default";
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public ICollection<ActorRole> ActorRoles { get; set; } = new List<ActorRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Resource { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Value => $"{Resource}:{Action}";
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class ActorRole
{
    public Guid ActorId { get; set; }
    public Actor Actor { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}