using Devor.Framework.Attributes;
using Devor.Framework.Models.Users;
using Devor.Framework.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UAParser;

namespace Devor.Framework.Services
{
    [ServiceRegisterar(Lifetime = ServiceLifetime.Singleton)]
    internal class UserStateService : IUserStateService
    {
        private readonly Dictionary<long, UserState> states = new Dictionary<long, UserState>();
        private readonly IGeoService geoService;
        private readonly AppSetting appSetting;

        public UserStateService(IGeoService geoService, AppSetting appSetting)
        {
            this.geoService = geoService;
            this.appSetting = appSetting;
        }

        public UserState GetUserState(long userId)
        {
            lock (states)
            {
                if (!states.TryGetValue(userId, out var result))
                {
                    result = new UserState(userId);
                    states[userId] = result;
                }
                else if (result.LastRequestTime < DateTime.Now.AddMinutes(-5))
                {
                    states.Remove(userId);
                    return GetUserState(userId);
                }
                return result;
            }
        }

        public IEnumerable<UserState> GetOnlines()
        {
            var time = DateTime.Now.AddMinutes(-1);
            return states.Values.Where(x => x?.LastRequestTime > time);
        }

        public void LoadClientInfo(UserState state, string uaString)
        {
            try
            {
                var uaParser = Parser.GetDefault();
                state.ClientInfo = uaParser.Parse(uaString);
            }
            catch { }
        }

        public void LoadGeoInfo(UserState state, string ip)
        {
            if (appSetting.Geo.Enable)
            {
                lock (state)
                {
                    if (state.IsGeoInfoInquiried) return;
                    state.IsGeoInfoInquiried = true;
                }
                new Thread(async () =>
                {
                    try
                    {
                        state.GeoInfo = await geoService.GetGeoInfoAsync(ip);
                    }
                    catch { }
                }).Start();
            }
        }
    }
}
