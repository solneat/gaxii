namespace Gax.Domain.Entities;

public class RoleRight
{
    public int RoleId { get; set; }
    public int RightId { get; set; }

    public Role Role { get; set; } = default!;
    public Right Right { get; set; } = default!;
}
