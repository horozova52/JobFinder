using AutoMapper;
using JobFinder.Core.Entities.Employers;
using JobFinder.Shared.DTOs.Employers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobFinder.UseCases.MappingProfiles.Profiles
{
    public class EmployersProfile : Profile
    {
        public EmployersProfile()
            {
            CreateMap<EmployerProfile, EmployerProfileDto>();
            CreateMap<CompanyLocation, CompanyLocationDto>().ReverseMap();
            CreateMap<UpdateEmployerProfileDto, EmployerProfile>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.UserId, opt => opt.Ignore())
               .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
               .ForMember(dest => dest.Locations, opt => opt.Ignore());
        }
    }
}
