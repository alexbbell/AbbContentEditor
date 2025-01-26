using AbbContentEditor.Models;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AbbAppContext _context;
        public DbSet<T> _dbSet;

        public Repository(AbbAppContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>(); // Initialize DbSet for the generic entity type
        }

        // Get all records (returns IQueryable to allow further filtering and querying)
        // Get all records with pagination
        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }

        // Get all records with pagination
        public IQueryable<T> GetAll(int pageNumber, int pageSize)
        {
            return _dbSet
                .Skip((pageNumber - 1) * pageSize) // Skip the records based on the page number
                .Take(pageSize); // Take only the number of records for the current page
        }
        // Find with filtering
        public IQueryable<T> Find(Func<IQueryable<T>, IQueryable<T>> filter = null, int pageNumber = 1, int pageSize = 10)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = filter(query); // Apply the filter
            }


            // Apply pagination
            return query
                .Skip((pageNumber - 1) * pageSize) // Skip the previous pages' records
                .Take(pageSize); // Take only the pageSize records
        }

        // Get a single entity by its primary key (assuming the key is int)
        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Add a new entity
        public async Task AddAsync(T entity)
        {
            if (typeof(T).IsSubclassOf(typeof(BaseClass)))
            {
                // Assuming entity is of type BaseClass or derived from it
                if (entity is BaseClass baseEntity)
                {
                    baseEntity.PubDate = DateTime.UtcNow;
                }
            }
            await _dbSet.AddAsync(entity);
        }

        // Update an existing entity
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
        }

        // Delete an entity
        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
        }

        //Delete entities by Clause
        public async Task<int> DeleteAsync(Func<IQueryable<T>, IQueryable<T>> filter = null)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null");

            var queryable = filter(_dbSet);

            if (queryable == null)
                throw new InvalidOperationException("The filter function returned null.");

            var entitiesToDelete = await queryable.ToListAsync();

            if (!entitiesToDelete.Any()) return 0;
                
            _dbSet.RemoveRange(entitiesToDelete);
            return entitiesToDelete.Count;
            //await _context.SaveChangesAsync();
        }


    }

}
