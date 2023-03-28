using snapnow.DTOS;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.DAOS;

public interface IUserDao : IDao<ApplicationUser, string>
{
    public DatabaseResponseModel<ApplicationUser> CheckPhoneNumber(string phoneNumber);
}