using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Words;

namespace AbbContentEditor.Data.UoW
{
    public class UnitOfWork :  IUnitOfWork 
    {
        private readonly AbbAppContext _context;
        public BlogRepository blogRepository { get; set; }
        public Repository<Category> categoryRepository {  get; set; }
        public Repository<BankOperation> bankOperationRepository {  get; set; }
        public Repository<WordHistory> wordHistoryRepository { get; set; }

        public UnitOfWork(AbbAppContext context)
        {
            _context = context;
            blogRepository  = new BlogRepository(_context);
            categoryRepository = new Repository<Category>(_context);
            bankOperationRepository  = new Repository<BankOperation>(_context);
            wordHistoryRepository = new Repository<WordHistory>(_context);
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


        public Task Rollback()
        {
            // Rollback anything, if necessary
            return Task.CompletedTask;
        }
    }
}

