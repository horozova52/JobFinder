using AutoMapper;
using JobFinder.Core.Entities.Employers;
using JobFinder.Shared.DTOs.Employers;
using System.Text.Json;

namespace JobFinder.UseCases.MappingProfiles.Profiles
{
    public class EmployersProfile : Profile
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EmployersProfile()
        {
            // Entity -> DTO (read)
            CreateMap<EmployerProfile, EmployerProfileDto>()
                .ForMember(dest => dest.Benefits, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.BenefitsJson)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(src.BenefitsJson, JsonOpts) ?? new List<string>()))
                .ForMember(dest => dest.RecruitmentProcess, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.RecruitmentProcessJson)
                        ? new List<RecruitmentStepDto>()
                        : JsonSerializer.Deserialize<List<RecruitmentStepDto>>(src.RecruitmentProcessJson, JsonOpts) ?? new List<RecruitmentStepDto>()));

            CreateMap<CompanyLocation, CompanyLocationDto>().ReverseMap();

            // UpdateDTO -> Entity (write)
            CreateMap<UpdateEmployerProfileDto, EmployerProfile>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.UserId, opt => opt.Ignore())
               .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
               .ForMember(dest => dest.Locations, opt => opt.Ignore())
               .ForMember(dest => dest.BenefitsJson, opt => opt.MapFrom(src =>
                   src.Benefits != null && src.Benefits.Count > 0
                       ? JsonSerializer.Serialize(src.Benefits, JsonOpts)
                       : null))
               .ForMember(dest => dest.RecruitmentProcessJson, opt => opt.MapFrom(src =>
                   src.RecruitmentProcess != null && src.RecruitmentProcess.Count > 0
                       ? JsonSerializer.Serialize(src.RecruitmentProcess, JsonOpts)
                       : null));
        }
    }
}
