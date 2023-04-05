using snapnow.Models;

namespace snapnow.Services;

public interface IMailService
{
    public Task SendEmailAsync(MailRequest mailRequest);
}