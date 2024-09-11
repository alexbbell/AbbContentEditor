using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Models;

namespace AbbContentEditor.Data.UoW
{
    public class UnitOfWork :  IUnitOfWork, IDisposable
    {
        private readonly AbbAppContext _context;
        public BlogRepository blogRepository { get; set; }
        public Repository<Category> categoryRepository {  get; set; }

        public UnitOfWork(AbbAppContext context)
        {
            _context = context;
            blogRepository  = new BlogRepository(_context);
            categoryRepository = new Repository<Category>(_context);
        }

        public Repository<Blog> BlogRepository
        {
            get
            {
                if (blogRepository == null) blogRepository = new BlogRepository(_context);
                return blogRepository;
            }
        }


        public async Task<bool> Commit()
        {
            bool success = false;
            try
            {
                _context.SaveChanges();
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            // var success = (await _context.SaveChangesAsync()) > 0;

            // Possibility to dispatch domain events, etc

            return success;
        }

        public void Dispose() =>
            _context.Dispose();

        public Task Rollback()
        {
            // Rollback anything, if necessary
            return Task.CompletedTask;
        }
    }
}

