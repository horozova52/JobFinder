using AutoMapper;
using JobFinder.Core.Entities.Common;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Shared.DTOs.Common;
using JobFinder.Shared.DTOs.Jobs;

namespace JobFinder.UseCases.MappingProfiles.Profiles
{
    public class CommonProfile : Profile
    {
        public CommonProfile()
        {
            CreateMap<Skill, SkillDto>().ReverseMap();
            CreateMap<Language, LanguageDto>().ReverseMap();
            CreateMap<JobCategory, JobCategoryDto>().ReverseMap();
        }
    }
}
