namespace Gax.Domain.Entities;

public class ResourceRight
{
    public int ResourceId { get; set; }
    public int RightId { get; set; }

    public Resource Resource { get; set; } = default!;
    public Right Right { get; set; } = default!;
}
