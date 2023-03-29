using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using snapnow.ErrorHandling;

namespace snapnow.DAOS;

public interface IDao<TResponseType, in TGetByParam, TAddResponseType>
{
    public Task<DatabaseResponseModel<TAddResponseType>> Add(TResponseType item, string temp = "");
    public Task<DatabaseResponseModel<TResponseType>> GetBy(TGetByParam validator);
    public DatabaseResponseModel<TResponseType> GetAll();
    public DatabaseResponseModel<TResponseType> Delete(TResponseType item);
}