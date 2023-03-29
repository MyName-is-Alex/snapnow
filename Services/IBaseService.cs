using snapnow.ErrorHandling;

namespace snapnow.Services;

public interface IBaseService<T, in TP>
{
    public Task<DatabaseResponseModel<T>> Add(TP item);
    public Task<DatabaseResponseModel<T>> GetBy(string id);
    public DatabaseResponseModel<T> GetAll();
    public DatabaseResponseModel<T> Delete(TP item);
}