using SolNeat.PolicyEngine;

public class Solution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Role> Roles { get; set; } = new List<Role>();
}