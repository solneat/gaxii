namespace Gax.Domain.Entities;

public class Right
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<RoleRight> RoleRights { get; set; } = new List<RoleRight>();
    public ICollection<ResourceRight> ResourceRights { get; set; } = new List<ResourceRight>();
}
