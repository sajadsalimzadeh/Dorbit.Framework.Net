﻿using UAParser.Objects;

namespace Dorbit.Models.Users;

public class UserState
{
    public Guid UserId { get; }
    public string Url { get; internal set; }
    public GeoInfo GeoInfo { get; internal set; }
    public ClientInfo ClientInfo { get; internal set; }
    public DateTime LastRequestTime { get; internal set; }
    public bool IsGeoInfoInquiry { get; internal set; }

    internal UserState()
    {
        LastRequestTime = DateTime.Now;
    }

    internal UserState(Guid userId) : this()
    {
        UserId = userId;

    }
}