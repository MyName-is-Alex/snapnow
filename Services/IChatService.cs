using snapnow.ErrorHandling;

namespace snapnow.Services;

public interface IChatService
{
    public Task<IBaseResponse> GeFollowedUsers(string currentUserEmail);
}