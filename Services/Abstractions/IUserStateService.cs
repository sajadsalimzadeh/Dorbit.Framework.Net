using Dorbit.Models.Users;

namespace Dorbit.Services.Abstractions;

public interface IUserStateService
{
    IEnumerable<UserState> GetOnlineUsers();
    UserState GetUserState(Guid userId);
    void LoadClientInfo(UserState state, string uaString);
    void LoadGeoInfo(UserState state, string ip);
}