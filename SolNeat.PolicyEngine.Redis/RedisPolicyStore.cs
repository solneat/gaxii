using StackExchange.Redis;
using System.Text.Json;

namespace SolNeat.PolicyEngine.Redis;

internal sealed class RedisPolicyStore : IPolicyStore
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Default;

    public RedisPolicyStore(IConnectionMultiplexer redis)
    {
        ArgumentNullException.ThrowIfNull(redis);
        _db = redis.GetDatabase();
    }

    public async Task<Actor?> FindActorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var json = await _db.StringGetAsync($"actor:id:{id}").ConfigureAwait(false);
        return json.HasValue ? JsonSerializer.Deserialize<Actor>(json.ToString()!, JsonOptions) : null;
    }

    public async Task<Actor?> FindActorBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(subjectId);

        var id = await _db.StringGetAsync($"actor:subject:{subjectId}").ConfigureAwait(false);
        if (!id.HasValue) return null;

        return await FindActorByIdAsync(Guid.Parse(id.ToString()!), cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsForActorAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(subjectId);

        var actor = await FindActorBySubjectIdAsync(subjectId, cancellationToken).ConfigureAwait(false);
        if (actor == null) return Array.Empty<Permission>();

        var roleIds = await _db.SetMembersAsync($"actor:{actor.Id}:roles").ConfigureAwait(false);
        if (roleIds.Length == 0) return Array.Empty<Permission>();

        var roleKeys = Array.ConvertAll(roleIds, r => (RedisKey)$"role:{r}:permissions");
        var permissionIds = await _db.SetCombineAsync(SetOperation.Union, roleKeys).ConfigureAwait(false);
        if (permissionIds.Length == 0) return Array.Empty<Permission>();

        var permKeys = Array.ConvertAll(permissionIds, p => (RedisKey)$"permission:id:{p}");
        var permJsons = await _db.StringGetAsync(permKeys).ConfigureAwait(false);

        var permissions = new List<Permission>(permJsons.Length);
        foreach (var json in permJsons)
        {
            if (json.HasValue)
            {
                var perm = JsonSerializer.Deserialize<Permission>(json.ToString()!, JsonOptions);
                if (perm != null) permissions.Add(perm);
            }
        }

        return permissions;
    }

    public async Task<Actor> GetOrCreateActorAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        var existing = await FindActorBySubjectIdAsync(subjectId, cancellationToken).ConfigureAwait(false);
        if (existing != null) return existing;

        return await CreateActorAsync(subjectId, subjectId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Actor> CreateActorAsync(string subjectId, string preferredName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(subjectId);

        var existing = await FindActorBySubjectIdAsync(subjectId, cancellationToken).ConfigureAwait(false);
        if (existing != null) return existing;

        var actor = new Actor { SubjectId = subjectId, PreferredName = preferredName };
        var json = JsonSerializer.Serialize(actor, JsonOptions);

        var tran = _db.CreateTransaction();
        _ = tran.StringSetAsync($"actor:id:{actor.Id}", json);
        _ = tran.StringSetAsync($"actor:subject:{subjectId}", actor.Id.ToString());

        var committed = await tran.ExecuteAsync().ConfigureAwait(false);
        if (!committed)
        {
            return (await FindActorBySubjectIdAsync(subjectId, cancellationToken).ConfigureAwait(false))!;
        }

        return actor;
    }

    public async Task<Permission> CreatePermissionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(resource);
        ArgumentException.ThrowIfNullOrEmpty(action);

        var existingId = await _db.StringGetAsync($"permission:lookup:{resource}:{action}").ConfigureAwait(false);
        if (existingId.HasValue)
        {
            var existingJson = await _db.StringGetAsync($"permission:id:{existingId}").ConfigureAwait(false);
            if (existingJson.HasValue)
            {
                return JsonSerializer.Deserialize<Permission>(existingJson.ToString()!, JsonOptions)!;
            }
        }

        var permission = new Permission { Resource = resource, Action = action };
        var json = JsonSerializer.Serialize(permission, JsonOptions);

        var tran = _db.CreateTransaction();
        _ = tran.StringSetAsync($"permission:id:{permission.Id}", json);
        _ = tran.StringSetAsync($"permission:lookup:{resource}:{action}", permission.Id.ToString());

        await tran.ExecuteAsync().ConfigureAwait(false);
        return permission;
    }

    public async Task<Solution> CreateSolutionAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var existingId = await _db.StringGetAsync($"solution:name:{name}").ConfigureAwait(false);
        if (existingId.HasValue)
        {
            var existingJson = await _db.StringGetAsync($"solution:id:{existingId}").ConfigureAwait(false);
            if (existingJson.HasValue)
            {
                return JsonSerializer.Deserialize<Solution>(existingJson.ToString()!, JsonOptions)!;
            }
        }

        var solution = new Solution { Name = name, Description = description };
        var json = JsonSerializer.Serialize(solution, JsonOptions);

        var tran = _db.CreateTransaction();
        _ = tran.StringSetAsync($"solution:id:{solution.Id}", json);
        _ = tran.StringSetAsync($"solution:name:{name}", solution.Id.ToString());

        await tran.ExecuteAsync().ConfigureAwait(false);
        return solution;
    }

    public async Task<Role> CreateRoleAsync(Guid solutionId, string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var role = new Role { SolutionId = solutionId, Name = name };
        var json = JsonSerializer.Serialize(role, JsonOptions);

        var tran = _db.CreateTransaction();
        _ = tran.StringSetAsync($"role:id:{role.Id}", json);
        _ = tran.StringSetAsync($"solution:{solutionId}:role:name:{name}", role.Id.ToString());
        _ = tran.SetAddAsync($"solution:{solutionId}:roles", role.Id.ToString());

        await tran.ExecuteAsync().ConfigureAwait(false);
        return role;
    }
    public async Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        await _db.SetAddAsync($"role:{roleId}:permissions", permissionId.ToString()).ConfigureAwait(false);
    }

    public async Task AssignRoleToActorAsync(Guid actorId, Guid roleId, CancellationToken cancellationToken = default)
    {
        await _db.SetAddAsync($"actor:{actorId}:roles", roleId.ToString()).ConfigureAwait(false);
    }

    // ==========================================
    // TOTP Operations (Redis Implementation)
    // ==========================================

    public async Task SetTotpSecretAsync(Guid actorId, string secret, bool isConfirmed = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);

        var existingJson = await _db.StringGetAsync($"totp:actor:{actorId}").ConfigureAwait(false);
        ActorTotpSecret totpSecret;

        if (existingJson.HasValue)
        {
            totpSecret = JsonSerializer.Deserialize<ActorTotpSecret>(existingJson.ToString()!, JsonOptions)!;
            totpSecret.Secret = secret;
            totpSecret.IsConfirmed = isConfirmed;
        }
        else
        {
            totpSecret = new ActorTotpSecret
            {
                ActorId = actorId,
                Secret = secret,
                IsConfirmed = isConfirmed,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        var json = JsonSerializer.Serialize(totpSecret, JsonOptions);
        await _db.StringSetAsync($"totp:actor:{actorId}", json).ConfigureAwait(false);
    }

    public async Task ConfirmTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        var json = await _db.StringGetAsync($"totp:actor:{actorId}").ConfigureAwait(false);
        if (!json.HasValue) return;

        var totpSecret = JsonSerializer.Deserialize<ActorTotpSecret>(json.ToString()!, JsonOptions);
        if (totpSecret == null) return;

        totpSecret.IsConfirmed = true;
        var updatedJson = JsonSerializer.Serialize(totpSecret, JsonOptions);

        await _db.StringSetAsync($"totp:actor:{actorId}", updatedJson).ConfigureAwait(false);
    }

    public async Task<ActorTotpSecret?> GetTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        var json = await _db.StringGetAsync($"totp:actor:{actorId}").ConfigureAwait(false);
        return json.HasValue ? JsonSerializer.Deserialize<ActorTotpSecret>(json.ToString()!, JsonOptions) : null;
    }

    public async Task RemoveTotpSecretAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync($"totp:actor:{actorId}").ConfigureAwait(false);
    }

    // ==========================================
    // Passkey / WebAuthn Operations (Redis Implementation)
    // ==========================================

    public async Task AddPasskeyAsync(ActorPasskey passkey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(passkey);

        var json = JsonSerializer.Serialize(passkey, JsonOptions);
        var credentialIdBase64 = Convert.ToBase64String(passkey.CredentialId);

        var tran = _db.CreateTransaction();
        _ = tran.StringSetAsync($"passkey:id:{passkey.Id}", json);
        _ = tran.StringSetAsync($"passkey:credential:{credentialIdBase64}", passkey.Id.ToString());
        _ = tran.SetAddAsync($"actor:{passkey.ActorId}:passkeys", passkey.Id.ToString());

        await tran.ExecuteAsync().ConfigureAwait(false);
    }

    public async Task<ActorPasskey?> GetPasskeyByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default)
    {
        if (credentialId == null || credentialId.Length == 0) return null;

        var credentialIdBase64 = Convert.ToBase64String(credentialId);
        var passkeyId = await _db.StringGetAsync($"passkey:credential:{credentialIdBase64}").ConfigureAwait(false);

        if (!passkeyId.HasValue) return null;

        var json = await _db.StringGetAsync($"passkey:id:{passkeyId}").ConfigureAwait(false);
        return json.HasValue ? JsonSerializer.Deserialize<ActorPasskey>(json.ToString()!, JsonOptions) : null;
    }

    public async Task<IReadOnlyCollection<ActorPasskey>> GetPasskeysByActorIdAsync(Guid actorId, CancellationToken cancellationToken = default)
    {
        var passkeyIds = await _db.SetMembersAsync($"actor:{actorId}:passkeys").ConfigureAwait(false);
        if (passkeyIds.Length == 0) return Array.Empty<ActorPasskey>();

        var keys = Array.ConvertAll(passkeyIds, id => (RedisKey)$"passkey:id:{id}");
        var jsons = await _db.StringGetAsync(keys).ConfigureAwait(false);

        var list = new List<ActorPasskey>(jsons.Length);
        foreach (var json in jsons)
        {
            if (json.HasValue)
            {
                var passkey = JsonSerializer.Deserialize<ActorPasskey>(json.ToString()!, JsonOptions);
                if (passkey != null) list.Add(passkey);
            }
        }

        return list;
    }

    public async Task UpdatePasskeyCounterAsync(Guid passkeyId, uint newCounter, CancellationToken cancellationToken = default)
    {
        var json = await _db.StringGetAsync($"passkey:id:{passkeyId}").ConfigureAwait(false);
        if (!json.HasValue) return;

        var passkey = JsonSerializer.Deserialize<ActorPasskey>(json.ToString()!, JsonOptions);
        if (passkey == null) return;

        passkey.SignatureCounter = newCounter;
        passkey.LastUsedAtUtc = DateTime.UtcNow;

        var updatedJson = JsonSerializer.Serialize(passkey, JsonOptions);
        await _db.StringSetAsync($"passkey:id:{passkeyId}", updatedJson).ConfigureAwait(false);
    }

    public async Task RemovePasskeyAsync(Guid passkeyId, CancellationToken cancellationToken = default)
    {
        var json = await _db.StringGetAsync($"passkey:id:{passkeyId}").ConfigureAwait(false);
        if (!json.HasValue) return;

        var passkey = JsonSerializer.Deserialize<ActorPasskey>(json.ToString()!, JsonOptions);
        if (passkey == null) return;

        var credentialIdBase64 = Convert.ToBase64String(passkey.CredentialId);

        var tran = _db.CreateTransaction();
        _ = tran.KeyDeleteAsync($"passkey:id:{passkeyId}");
        _ = tran.KeyDeleteAsync($"passkey:credential:{credentialIdBase64}");
        _ = tran.SetRemoveAsync($"actor:{passkey.ActorId}:passkeys", passkeyId.ToString());

        await tran.ExecuteAsync().ConfigureAwait(false);
    }
    public async Task<Solution?> FindSolutionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var json = await _db.StringGetAsync($"solution:id:{id}").ConfigureAwait(false);
        return json.HasValue ? JsonSerializer.Deserialize<Solution>(json.ToString()!, JsonOptions) : null;
    }

    public async Task<Solution?> FindSolutionByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var json = await _db.StringGetAsync($"solution:name:{name}").ConfigureAwait(false);
        return json.HasValue ? JsonSerializer.Deserialize<Solution>(json.ToString()!, JsonOptions) : null;
    }
}