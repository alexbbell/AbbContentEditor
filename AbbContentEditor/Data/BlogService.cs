using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Models;

namespace AbbContentEditor.Data
{
    public interface IBlogService
    {
        IRepository<Blog> BlogRepository { get; }
        IRepository<Category> CategoryRepository { get; }
        void SaveChanges();
        Blog AddBlog(Blog item);
        Blog GetBlogById(int id);
    }

    public class BlogService : IBlogService
    {
        private readonly AbbAppContext _appContext;
        public IRepository<Blog> BlogRepository { get; }
        public IRepository<Category> CategoryRepository { get; }

        public BlogService(AbbAppContext appContext, IRepository<Blog> blogRepository, IRepository<Category> categoryRepository)
        {
            // _appContext = appContext;
            BlogRepository = blogRepository ?? throw new ArgumentNullException(nameof(blogRepository));
            CategoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public virtual Blog AddBlog(Blog item)
        {
            BlogRepository.Add(item);
            return item;
        }


        public virtual Blog GetBlogById(int id)
        {
            var blog = BlogRepository.GetById(id);
            return blog;
        }

        public virtual void SaveChanges()
        {
            _appContext.SaveChanges();
        }

        public void Dispose()
        {
            _appContext.Dispose();
        }

       
    }
}
