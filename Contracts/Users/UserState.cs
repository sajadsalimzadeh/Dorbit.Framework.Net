using System;
using UAParser.Objects;

namespace Dorbit.Framework.Models.Users;

public class UserState
{
    public Guid UserId { get; }
    public string Url { get; internal set; }
    public GeoInfo GeoInfo { get; internal set; }
    public ClientInfo ClientInfo { get; internal set; }
    public DateTime LastRequestTime { get; internal set; } = DateTime.UtcNow;
    public bool IsGeoInfoInquiry { get; internal set; }


    internal UserState(Guid userId)
    {
        UserId = userId;

    }
}