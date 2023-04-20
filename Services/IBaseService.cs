using snapnow.ErrorHandling;

namespace snapnow.Services;

public interface IBaseService<TResponse, in TAddParameter, TAddResponse>
{
    public Task<DatabaseResponseModel<TAddResponse>> Add(TAddParameter item);
    public Task<DatabaseResponseModel<TResponse>> GetBy(string id);
    public DatabaseResponseModel<List<TResponse>> GetAll();
    public DatabaseResponseModel<TResponse> Delete(TAddParameter item);
}