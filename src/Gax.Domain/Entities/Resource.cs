namespace Gax.Domain.Entities;

public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<ResourceRight> ResourceRights { get; set; } = new List<ResourceRight>();
}
