using System;
using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Jobs;

public class JobDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? CancelTime { get; set; }
    public DateTime? EndTime { get; set; }
    public JobStatus Status { get; set; }
    public bool Pausable { get; set; }
    public bool Downloadable { get; set; }
    public string DownloadFilename { get; set; }
    public string Step { get; set; }
    public double Progress { get; set; }
    public List<Job.AuditLog> AuditLogs { get; set; }
}