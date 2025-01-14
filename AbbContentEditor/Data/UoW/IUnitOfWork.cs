using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Models;

namespace AbbContentEditor.Data.UoW
{
    public interface IUnitOfWork 
    {
        Repository<Category> categoryRepository { get; set; }
        Repository<WordHistory> wordHistoryRepository { get; set; }
        Repository<BankOperation> bankOperationRepository { get; set; }
        BlogRepository blogRepository { get; }
        Task<bool> Commit();
        Task Rollback();
    }
}
