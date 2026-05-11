using JobFinder.Core.Entities.Candidates;
using JobFinder.Core.Entities.Employers;
using JobFinder.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace JobFinder.Core.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public UserType UserType { get; set; }
    public CandidateProfile? CandidateProfile { get; set; }
    public EmployerProfile? EmployerProfile { get; set; }
   
}
