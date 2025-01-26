namespace AbbContentEditor.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetAll(int pageNumber, int pageSize);

        IQueryable<T> Find(
                  Func<IQueryable<T>, IQueryable<T>> filter = null,
                  int pageNumber = 1,
                  int pageSize = 10);
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> DeleteAsync(Func<IQueryable<T>, IQueryable<T>> filter = null);
    }
}
