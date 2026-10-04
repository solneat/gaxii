using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace SolNeat.PolicyEngine.OpenIddict;

public interface ILdapAuthenticator
{
    /// <summary>
    /// Authenticates credentials against LDAP server and provisions or updates the corresponding PolicyEngine Actor.
    /// </summary>
    Task<Actor?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}

internal sealed class LdapAuthenticator : ILdapAuthenticator
{
    private readonly LdapOptions _options;
    private readonly IPolicyStore _policyStore;

    public LdapAuthenticator(IOptions<AuthenticationPipelineOptions> options, IPolicyStore policyStore)
    {
        _options = options.Value.Ldap ?? throw new ArgumentNullException(nameof(options));
        _policyStore = policyStore ?? throw new ArgumentNullException(nameof(policyStore));
    }

    public async Task<Actor?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        using var connection = new LdapConnection
        {
            SecureSocketLayer = _options.UseSsl
        };

        try
        {
            // Connect asynchronously to LDAP server
            await connection.ConnectAsync(_options.Host, _options.Port, cancellationToken).ConfigureAwait(false);

            await connection.BindAsync(_options.BindDn, _options.BindPassword, cancellationToken).ConfigureAwait(false);

            // Find user distinguished name (DN)
            var searchFilter = $"({_options.UserAttribute}={username})";
            var searchResults = await connection.SearchAsync(
                _options.BaseDn,
                LdapConnection.ScopeSub,
                searchFilter,
                _options.SearchAttributes,
                typesOnly: false,
                ct: cancellationToken
            ).ConfigureAwait(false);

            LdapEntry? userEntry = null;

            // Iterate asynchronously through search results using HasNext()
            while (await searchResults.HasMoreAsync(ct: cancellationToken))
            {
                userEntry = await searchResults.NextAsync(ct: cancellationToken);
                if (userEntry != null) break;
            }

            if (userEntry == null)
                return null;

            var userDn = userEntry.Dn;

            // Validate user password against LDAP
            using var userConnection = new LdapConnection
            {
                SecureSocketLayer = _options.UseSsl
            };

            await userConnection.ConnectAsync(_options.Host, _options.Port, cancellationToken).ConfigureAwait(false);
            
            await userConnection.BindAsync(userDn, password, cancellationToken).ConfigureAwait(false);

            if (!userConnection.Bound)
                return null;

            // Provision or Fetch Actor
            var attributeSet = userEntry.GetAttributeSet();

            string displayName;
            if (attributeSet.TryGetValue("displayname", out var displayNameAttr)) 
                displayName = displayNameAttr.StringValue;
            else 
                displayName = username;
            
            var actor = await _policyStore.CreateActorAsync(username, displayName, cancellationToken).ConfigureAwait(false);

            // Sync LDAP Groups to Roles if configured
            if (_options.SyncRolesFromLdapGroups && attributeSet.TryGetValue("memberOf", out var groupAttr))
            {
                var groupDns = groupAttr?.StringValueArray;

                if (groupAttr != null)
                {
                    foreach (var groupDn in groupAttr.StringValueArray)
                    {
                        var roleName = ExtractGroupNameFromDn(groupDn);
                        if (!string.IsNullOrEmpty(roleName))
                        {
                            var solution = await _policyStore.FindSolutionByNameAsync(_options.DefaultSolutionName, cancellationToken);
                            if (solution == null)
                            {
                                solution = await _policyStore.CreateSolutionAsync(_options.DefaultSolutionName, cancellationToken:  cancellationToken);
                            }

                            var role = await _policyStore.CreateRoleAsync(solution.Id, roleName, cancellationToken).ConfigureAwait(false);
                            await _policyStore.AssignRoleToActorAsync(actor.Id, role.Id, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }

            return actor;
        }
        catch (LdapException)
        {
            return null;
        }
    }

    private static string ExtractGroupNameFromDn(string dn)
    {
        var parts = dn.Split(',');
        foreach (var part in parts)
        {
            if (part.Trim().StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
            {
                return part.Trim()[3..];
            }
        }
        return string.Empty;
    }
}