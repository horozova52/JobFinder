using AutoMapper;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Shared.DTOs.Jobs;

namespace JobFinder.UseCases.MappingProfiles.Profiles
{
    public class JobsProfile : Profile
    {
        public JobsProfile()
        {
            CreateMap<JobPosting, JobPostingDto>()
                .ForMember(d => d.Skills, opt => opt.MapFrom(s => s.Skills))
                .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.EmployerProfile.CompanyName));

            CreateMap<JobSkill, JobSkillDto>()
                .ForMember(d => d.SkillName, opt => opt.MapFrom(s => s.Skill.Name));
        }
    }
}
