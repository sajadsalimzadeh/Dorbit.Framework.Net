﻿using System;
using Dorbit.Framework.Utils;

namespace Dorbit.Framework.Contracts.Users;

public class UserState
{
    public string UserId { get; }
    public string Url { get; internal set; }
    public GeoInfo GeoInfo { get; internal set; }
    public ClientInfo ClientInfo { get; internal set; }
    public DateTime LastRequestTime { get; internal set; } = DateTime.UtcNow;
    public bool IsGeoInfoInquiry { get; internal set; }


    internal UserState(string userId)
    {
        UserId = userId;
    }
}