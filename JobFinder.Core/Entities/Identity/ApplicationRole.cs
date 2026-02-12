using Microsoft.AspNetCore.Identity;

namespace JobFinder.Core.Entities.Identity;

public class ApplicationRole : IdentityRole
{
   public string? Description { get; set; }
}
