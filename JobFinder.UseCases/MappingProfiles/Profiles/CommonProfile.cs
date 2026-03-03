using AutoMapper;
using JobFinder.Core.Entities.Common;
using JobFinder.Shared.DTOs.Common;

namespace JobFinder.UseCases.MappingProfiles.Profiles
{
    public class CommonProfile : Profile
    {
        public CommonProfile()
        {
            CreateMap<Skill, SkillDto>().ReverseMap();
            CreateMap<Language, LanguageDto>().ReverseMap();
        }
    }
}
