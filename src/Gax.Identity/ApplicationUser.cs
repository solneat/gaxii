using Microsoft.AspNetCore.Identity;

namespace Gax.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string? FullName { get; set; }
}
