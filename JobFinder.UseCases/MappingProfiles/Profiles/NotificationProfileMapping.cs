using AutoMapper;
using JobFinder.Core.Entities.Common;
using JobFinder.Shared.DTOs.Common;

namespace JobFinder.Server.Mapping;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}
