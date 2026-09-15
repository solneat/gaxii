using Microsoft.AspNetCore.Identity;

namespace Gax.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public string? Description { get; set; }
}
