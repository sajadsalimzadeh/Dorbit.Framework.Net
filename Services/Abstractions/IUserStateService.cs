using System.Collections.Generic;
using Dorbit.Framework.Contracts.Users;

namespace Dorbit.Framework.Services.Abstractions;

public interface IUserStateService
{
    IEnumerable<UserState> GetOnlineUsers();
    UserState GetUserState(string userId);
    void LoadClientInfo(UserState state, string uaString);
    void LoadGeoInfo(UserState state, string ip);
}