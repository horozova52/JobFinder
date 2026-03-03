using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AutoMapper;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Shared.DTOs.Candidates;

namespace JobFinder.UseCases.MappingProfiles.Profiles
{
    public class CandidateMappingProfile : Profile
    {
        public CandidateMappingProfile()
        {
            CreateMap<CandidateMappingProfile, CandidateProfileDto>();

            CreateMap<Experience, ExperienceDto>().ReverseMap();
            CreateMap<Education, EducationDto>().ReverseMap();
            CreateMap<CandidateSkill, CandidateSkillDto>()
                .ForMember(d => d.SkillName, opt => opt.MapFrom(s => s.Skill.Name));
            CreateMap<Certification, CertificationDto>().ReverseMap();
            CreateMap<CandidateLanguage, CandidateLanguageDto>()
                .ForMember(d => d.LanguageName, opt => opt.MapFrom(s => s.Language.Name));
        }
    }
}