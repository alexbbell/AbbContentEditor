using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Models;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Data
{
    public interface IUserService
    {
        IRepository<Blog> BlogRepository { get; }
        IRepository<Category> CategoryRepository { get; }
        void SaveChanges();
    }

    public class UserService : IUserService
    {
        private readonly AbbAppContext _appContext;
        private IRepository<Blog> _blogRepository;
        private IRepository<Category> _categoryRepository;

        public UserService(AbbAppContext appContext)
        {
            _appContext = appContext;
        }

        public IRepository<Blog> BlogRepository
        {
            get
            {
                if (_blogRepository == null)
                {
                    _blogRepository = new Repository<Blog>(_appContext);
                }
                return _blogRepository;
            }
        }

        public IRepository<Category> CategoryRepository
        {
            get
            {
                if (_categoryRepository == null)
                {
                    _categoryRepository = new Repository<Category>(_appContext);
                }
                return _categoryRepository;
            }
        }

        public void SaveChanges()
        {
            _appContext.SaveChanges();
        }
        public void Dispose()
        {
            _appContext.Dispose();
        }
    }
}
