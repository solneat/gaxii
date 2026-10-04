namespace SolNeat.PolicyEngine.EF;

using Microsoft.EntityFrameworkCore;
using SolNeat.PolicyEngine;

internal sealed class PolicyStore<TContext> : IPolicyStore
    where TContext : DbContext
{
    private readonly TContext _context;

    public PolicyStore(TContext context) => _context = context;

    public async Task<Actor?> FindActorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Actor>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Actor?> FindActorBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Actor>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.SubjectId == subjectId, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsForActorAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Permission>()
            .AsNoTracking()
            .Where(p => p.RolePermissions.Any(rp => rp.Role.ActorRoles.Any(ar => ar.Actor.SubjectId == subjectId)))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<Actor> GetOrCreateActorAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        var actor = await _context.Set<Actor>().FirstOrDefaultAsync(a => a.SubjectId == subjectId, cancellationToken);
        if (actor != null) return actor;

        actor = new Actor { SubjectId = subjectId, PreferredName = subjectId };
        _context.Set<Actor>().Add(actor);
        await _context.SaveChangesAsync(cancellationToken);
        return actor;
    }

    public async Task<Actor> CreateActorAsync(string subjectId, string preferredName, CancellationToken cancellationToken = default)
    {
        var actor = await _context.Set<Actor>().FirstOrDefaultAsync(a => a.SubjectId == subjectId, cancellationToken);
        if (actor != null) return actor;

        actor = new Actor { SubjectId = subjectId, PreferredName = preferredName };
        _context.Set<Actor>().Add(actor);
        await _context.SaveChangesAsync(cancellationToken);
        return actor;
    }

    public async Task<Permission> CreatePermissionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Set<Permission>()
            .FirstOrDefaultAsync(p => p.Resource == resource && p.Action == action, cancellationToken);

        if (permission != null) return permission;

        permission = new Permission { Resource = resource, Action = action };
        _context.Set<Permission>().Add(permission);
        await _context.SaveChangesAsync(cancellationToken);
        return permission;
    }

    // ==========================================
    // Solution Operations
    // ==========================================

    public async Task<Solution> CreateSolutionAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var solution = await _context.Set<Solution>()
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);

        if (solution != null) return solution;

        solution = new Solution { Name = name, Description = description };
        _context.Set<Solution>().Add(solution);
        await _context.SaveChangesAsync(cancellationToken);
        return solution;
    }

    public async Task<Solution?> FindSolutionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Solution>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
    
    public async Task<Solution?> FindSolutionByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Solution>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    // ==========================================
    // Role Operations (Scoped to Solution)
    // ==========================================

    public async Task<Role> CreateRoleAsync(Guid solutionId, string name, CancellationToken cancellationToken = default)
    {
        var role = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.SolutionId == solutionId && r.Name == name, cancellationToken);

        if (role != null) return role;

        role = new Role { SolutionId = solutionId, Name = name };
        _context.Set<Role>().Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Set<RolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (!exists)
        {
            _context.Set<RolePermission>().Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AssignRoleToActorAsync(Guid actorId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Set<ActorRole>()
            .AnyAsync(ar => ar.ActorId == actorId && ar.RoleId == roleId, cancellationToken);

        if (!exists)
        {
            _context.Set<ActorRole>().Add(new ActorRole { ActorId = actorId, RoleId = roleId });
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // ==========================================
    // TOTP Operations
    // ==========================================

    public async Task SetTotpSecretAsync(Guid actorId, string secret, bool isConfirmed = false, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Set<ActorTotpSecret>()
            .FirstOrDefaultAsync(t => t.ActorId == actorId, cancellationToken);

        if (existing == null)
        {
            var totp = new ActorTotpSecret
            {
                ActorId = actorId,
                Secret = secret,
                IsConfirmed = isConfirmed,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.Set<ActorTotpSecret>().Add(totp);
        }
        else
        {
            existing.Secret = secret;
            existing.IsConfirmed = isConfirmed;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Set<ActorTotpSecret>()
            .FirstOrDefaultAsync(t => t.ActorId == actorId, cancellationToken);

        if (existing != null)
        {
            existing.IsConfirmed = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ActorTotpSecret?> GetTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ActorTotpSecret>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ActorId == actorId, cancellationToken);
    }

    public async Task RemoveTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Set<ActorTotpSecret>()
            .FirstOrDefaultAsync(t => t.ActorId == actorId, cancellationToken);

        if (existing != null)
        {
            _context.Set<ActorTotpSecret>().Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // ==========================================
    // Passkey / WebAuthn Operations
    // ==========================================

    public async Task AddPasskeyAsync(ActorPasskey passkey, CancellationToken cancellationToken = default)
    {
        _context.Set<ActorPasskey>().Add(passkey);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ActorPasskey?> GetPasskeyByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ActorPasskey>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CredentialId.SequenceEqual(credentialId), cancellationToken);
    }

    public async Task<IReadOnlyCollection<ActorPasskey>> GetPasskeysByActorIdAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ActorPasskey>()
            .AsNoTracking()
            .Where(p => p.ActorId == actorId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdatePasskeyCounterAsync(Guid passkeyId, uint newCounter, CancellationToken cancellationToken = default)
    {
        var passkey = await _context.Set<ActorPasskey>()
            .FirstOrDefaultAsync(p => p.Id == passkeyId, cancellationToken);

        if (passkey != null)
        {
            passkey.SignatureCounter = newCounter;
            passkey.LastUsedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemovePasskeyAsync(Guid passkeyId, CancellationToken cancellationToken = default)
    {
        var passkey = await _context.Set<ActorPasskey>()
            .FirstOrDefaultAsync(p => p.Id == passkeyId, cancellationToken);

        if (passkey != null)
        {
            _context.Set<ActorPasskey>().Remove(passkey);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}