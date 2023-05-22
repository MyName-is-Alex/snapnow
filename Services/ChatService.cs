using snapnow.DAOS;
using snapnow.ErrorHandling;

namespace snapnow.Services;

public class ChatService : IChatService
{
    private readonly IUserDao _userDao;

    public ChatService(IUserDao userDao)
    {
        _userDao = userDao;
    }

    public async Task<IBaseResponse> GeFollowedUsers(string currentUserEmail)
    {
        var databaseResponse = await _userDao.GetFollowedUsers(currentUserEmail);
        return databaseResponse;
    }
}