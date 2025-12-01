using System;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobStatusDto
{
    public Guid Id { get; set; }
    public string Step { get; set; }
    public double Progress { get; set; }
    public JobStatus Status { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? CancelTime { get; set; }
}