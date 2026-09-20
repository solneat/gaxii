using System.Security.Claims;

namespace SolNeat.PolicyEngine;

public interface IPolicyDecisionPoint
{
    Task<IReadOnlyList<string>> EvaluateAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public sealed class PolicyDecisionPoint : IPolicyDecisionPoint
{
    private readonly IPolicyStore _store;
    public PolicyDecisionPoint(IPolicyStore store) => _store = store;

    public async Task<IReadOnlyList<string>> EvaluateAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var subjectId = principal.FindFirst("sub")?.Value
                        ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(subjectId)) return Array.Empty<string>();

        var permissions = await _store.GetPermissionsForActorAsync(subjectId, cancellationToken);
        return permissions.Select(p => p.Value).Distinct().ToList();
    }
}