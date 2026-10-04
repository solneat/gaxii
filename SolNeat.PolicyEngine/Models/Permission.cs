namespace SolNeat.PolicyEngine;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Resource { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Value => $"{Resource}:{Action}";
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
