using System;
using AutoMapper;
using Dorbit.Framework.Contracts.Jobs;

namespace Dorbit.Framework.Mappers;

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<Job, JobDto>()
            .ForMember(x => x.Downloadable, o => o.MapFrom(x => x.Download != null));
        CreateMap<JobLog, JobLogDto>();
    }
}