namespace SolNeat.PolicyEngine.OpenIddict;

public sealed class LdapOptions
{
    /// <summary>
    /// Gets or sets the LDAP server hostname or IP address.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// Gets or sets the LDAP server port 
    /// </summary>
    public int Port { get; set; } = 7751;

    /// <summary>
    /// Gets or sets a value indicating whether SSL/TLS connection is enabled.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Gets or sets the Base DN for searching users (e.g., "ou=users,dc=company,dc=com").
    /// </summary>
    public string BaseDn { get; set; } = "ou=people,dc=example,dc=com";

    /// <summary>
    /// Gets or sets the Bind DN for service account authentication (optional for anonymous search).
    /// </summary>
    public string BindDn { get; set; } = "uid=ldap_reader,ou=people,dc=example,dc=com";

    /// <summary>
    /// Gets or sets the password for the service account.
    /// </summary>
    public string BindPassword { get; set; } = "Pass1234";

    /// <summary>
    /// 
    /// </summary>
    public string UserAttribute { get; set; } = "uid";

    /// <summary>
    /// 
    /// </summary>
    public string[] SearchAttributes { get; set; } =
    {
        "displayname",
        "firstname",
        "lastname",
        "department",
        "mail",
        "memberOf"
    };

    /// <summary>
    /// Gets or sets a value indicating whether LDAP groups should be mapped to Policy Engine Roles.
    /// </summary>
    public bool SyncRolesFromLdapGroups { get; set; } = true;

    public string DefaultSolutionName { get; set; } = "Main";
}