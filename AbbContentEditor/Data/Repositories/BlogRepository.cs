using AbbContentEditor.Models;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Data.Repositories
{
    public class BlogRepository : Repository<Blog>
    {
        public BlogRepository(DbContext context) : base(context)
        {
        }
        public IQueryable<Blog> GetPaginatedStudentsWithCategory(int pageNumber, int pageSize)
        {
            return _dbSet
                .Include(s => s.Category)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
    }
}
