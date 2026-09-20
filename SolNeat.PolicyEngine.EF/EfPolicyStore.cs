using Microsoft.EntityFrameworkCore;

namespace SolNeat.PolicyEngine.EF;

internal sealed class EfPolicyStore<TContext> : IPolicyStore
    where TContext : DbContext
{
    private readonly TContext _context;

    public EfPolicyStore(TContext context) => _context = context;

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

        actor = new Actor { SubjectId = subjectId };
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

    public async Task<Role> CreateRoleAsync(string name, CancellationToken cancellationToken = default)
    {
        var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
        if (role != null) return role;

        role = new Role { Name = name };
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
}