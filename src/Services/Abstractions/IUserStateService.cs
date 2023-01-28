using Devor.Framework.Models.Users;
using System.Collections.Generic;

namespace Devor.Framework.Services.Abstractions
{
    public interface IUserStateService
    {
        IEnumerable<UserState> GetOnlines();
        UserState GetUserState(long userId);
        void LoadClientInfo(UserState state, string uaString);
        void LoadGeoInfo(UserState state, string ip);
    }
}
