using AutoMapper;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Shared.DTOs.Jobs;

namespace JobFinder.UseCases.MappingProfiles.Profiles
{
    public class JobsProfile : Profile
    {
        public JobsProfile()
        {
            CreateMap<JobPosting, JobPostingDto>();
            CreateMap<JobSkill, JobSkillDto>()
                .ForMember(d => d.SkillName, opt => opt.MapFrom(s => s.Skill.Name));
            CreateMap<JobCategory, JobCategoryDto>().ReverseMap();
        }
    }
}
