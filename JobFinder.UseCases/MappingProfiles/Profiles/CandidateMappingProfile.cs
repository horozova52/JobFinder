using AutoMapper;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Shared.DTOs.Candidates;

namespace JobFinder.UseCases.MappingProfiles.Profiles;

public class CandidateMappingProfile : Profile
{
    public CandidateMappingProfile()
    {
        // CandidateProfile → CandidateProfileDto
        CreateMap<CandidateProfile, CandidateProfileDto>();

        // UpdateCandidateProfileDto → CandidateProfile
        CreateMap<UpdateCandidateProfileDto, CandidateProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.Experiences, opt => opt.Ignore())
            .ForMember(dest => dest.Educations, opt => opt.Ignore())
            .ForMember(dest => dest.Skills, opt => opt.Ignore())
            .ForMember(dest => dest.Certifications, opt => opt.Ignore())
            .ForMember(dest => dest.Languages, opt => opt.Ignore());

        // Experience → ExperienceDto
        CreateMap<Experience, ExperienceDto>();

        // Education → EducationDto
        CreateMap<Education, EducationDto>();

        // CandidateSkill → CandidateSkillDto
        CreateMap<CandidateSkill, CandidateSkillDto>()
            .ForMember(dest => dest.SkillName, opt => opt.MapFrom(src => src.Skill.Name))
            .ForMember(dest => dest.SkillId, opt => opt.MapFrom(src => src.SkillId));

        // CandidateLanguage → CandidateLanguageDto
        CreateMap<CandidateLanguage, CandidateLanguageDto>()
            .ForMember(dest => dest.LanguageName, opt => opt.MapFrom(src => src.Language.Name))
            .ForMember(dest => dest.LanguageId, opt => opt.MapFrom(src => src.LanguageId));

        // Certification → CertificationDto
        CreateMap<Certification, CertificationDto>();
    }
}