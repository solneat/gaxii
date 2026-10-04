namespace SolNeat.PolicyEngine;

public class Actor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PreferredName { get; set; } = null!;
    public string SubjectId { get; set; } = null!; // sub или client_id из OpenIddict
    public ICollection<ActorRole> ActorRoles { get; set; } = new List<ActorRole>();
}
