using VismaBookLibrary.Models;

namespace VismaBookLibrary.DAL
{
    public interface IUnitOfWork
    {
        IRepository<Book> Books { get; }
        IRepository<Reader> Readers { get; }
        IRepository<Lending> Lendings { get; }
    }
}