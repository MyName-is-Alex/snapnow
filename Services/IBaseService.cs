using snapnow.ErrorHandling;

namespace snapnow.Services;

public interface IBaseService<T, in TP>
{
    public Task<DatabaseResponseModel<T>> Add(TP item);
    public DatabaseResponseModel<T> GetBy(int id);
    public DatabaseResponseModel<T> GetAll();
    public DatabaseResponseModel<T> Delete(TP item);
}