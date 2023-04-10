using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.Services;

public interface IMailService
{
    public Task<UserResponseModel> SendEmailAsync(MailRequest mailRequest);
}