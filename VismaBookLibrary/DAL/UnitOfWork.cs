using VismaBookLibrary.Models;

namespace VismaBookLibrary.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        public IRepository<Book> Books { get; }
        public IRepository<Reader> Readers { get; }
        public IRepository<Lending> Lendings { get; }

        public UnitOfWork()
        {
            Books = new Repository<Book>("../../../DAL/Data/Books.json",
                                         "../../../DAL/Data/LastIds/LastBookId.txt");
            Readers = new Repository<Reader>("../../../DAL/Data/Readers.json",
                                             "../../../DAL/Data/LastIds/LastReaderId.txt");
            Lendings = new Repository<Lending>("../../../DAL/Data/Lendings.json",
                                               "../../../DAL/Data/LastIds/LastLendingId.txt");
        }
    }
}