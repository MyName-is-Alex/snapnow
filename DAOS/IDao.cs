using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using snapnow.ErrorHandling;

namespace snapnow.DAOS;

public interface IDao<T>
{
    public Task<DatabaseResponseModel<T>> Add(T item);
    public DatabaseResponseModel<T> GetBy(int id);
    public DatabaseResponseModel<T> GetAll();
    public DatabaseResponseModel<T> Delete(T item);
}