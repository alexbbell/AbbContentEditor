using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Models;

namespace AbbContentEditor.Data.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        BlogRepository blogRepository { get; }
        Task<bool> Commit();
        Task Rollback();
    }
}
