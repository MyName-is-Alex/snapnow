using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using snapnow.ErrorHandling;

namespace snapnow.DAOS;

public interface IDao<T, in TP>
{
    public Task<DatabaseResponseModel<T>> Add(T item, string temp = "");
    public DatabaseResponseModel<T> GetBy(TP validator);
    public DatabaseResponseModel<T> GetAll();
    public DatabaseResponseModel<T> Delete(T item);
}