using AutoMapper;
using Dorbit.Framework.Contracts;
using Dorbit.Framework.Contracts.Jobs;
using Dorbit.Framework.Contracts.Notifications;
using Dorbit.Framework.Entities;

namespace Dorbit.Framework.Mappers;

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<Job, JobDto>()
            .ForMember(x => x.Downloadable, o => o.MapFrom(x => x.Download != null));
        CreateMap<JobLog, JobLogDto>();
        
        CreateMap<NotificationDto, Notification>();
        CreateMap<Notification, NotificationDto>();
    }
}