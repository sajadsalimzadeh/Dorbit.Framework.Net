using System;
using UAParser;

namespace Devor.Framework.Models.Users
{
    public class UserState
    {
        public long UserId { get; }
        public string Url { get; internal set; }
        public GeoInfo GeoInfo { get; internal set; }
        public ClientInfo ClientInfo { get; internal set; }
        public DateTime LastRequestTime { get; internal set; }
        public bool IsGeoInfoInquiried { get; internal set; }

        internal UserState()
        {
            LastRequestTime = DateTime.Now;
        }

        internal UserState(long userId) : this()
        {
            UserId = userId;

        }
    }
}
