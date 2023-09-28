namespace HistoryService.Data
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Get(Guid id);
        T Add(T entity);
    }
}
