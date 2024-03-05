using System;
using AutoMapper;
using Dorbit.Framework.Contracts.Jobs;

namespace Dorbit.Framework.Mappers;

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<Job, JobDto>();
        CreateMap<JobLog, JobLogDto>();
    }
}