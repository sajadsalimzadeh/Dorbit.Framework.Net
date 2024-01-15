using System;
using System.Collections.Generic;
using Dorbit.Framework.Models.Users;

namespace Dorbit.Framework.Services.Abstractions;

public interface IUserStateService
{
    IEnumerable<UserState> GetOnlineUsers();
    UserState GetUserState(Guid userId);
    void LoadClientInfo(UserState state, string uaString);
    void LoadGeoInfo(UserState state, string ip);
}