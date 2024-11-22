using AbbContentEditor.Models;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Data.Repositories
{
    public class BlogRepository : Repository<Blog> 
    {
        //private readonly DbContext _context;
        //private readonly DbSet<Blog> _dbSet;
        
        public BlogRepository(AbbAppContext context) : base(context)
        {
            //_dbSet = _context.Set<Blog>(); // Initialize DbSet for the generic entity type
        }
        public IQueryable<Blog> GetPaginatedBlogsWithCategory(int pageNumber, int pageSize)
        {
            return _dbSet
                .Include(s => s.Category)
                .OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        public Blog GetBlogListItem(int id)
        {
            Blog res = _dbSet.Include(s => s.Category).FirstOrDefault(x => x.Id == id);
            return res;
        }
    }
}
