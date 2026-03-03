using AutoMapper;
using JobFinder.Core.Entities.Applications;
using JobFinder.Shared.DTOs.Applications;

namespace JobFinder.UseCases.MappingProfiles.Profiles
{
    public class ApplicationsProfile : Profile
    {
        public ApplicationsProfile()
        {
            CreateMap<Application, ApplicationDto>()
               .ForMember(d => d.JobTitle, opt => opt.MapFrom(s => s.JobPosting.Title))
               .ForMember(d => d.CandidateFullName,
                   opt => opt.MapFrom(s => s.CandidateProfile.FirstName + " " + s.CandidateProfile.LastName));

            CreateMap<ApplicationStatusHistory, ApplicationStatusHistoryDto>();
        }

    }
}
