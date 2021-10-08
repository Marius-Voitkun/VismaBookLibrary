using System;
using System.Linq;
using System.Threading.Tasks;
using VismaBookLibrary.DAL;
using VismaBookLibrary.Models;

namespace VismaBookLibrary.Services
{
    public class LendingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly short _maxLendingPeriodInDays;
        private readonly byte _maxNumberOfBooksPerReader;

        public LendingsService(IUnitOfWork unitOfWork, short maxLendingPeriodInDays, byte maxNumberOfBooksPerReader)
        {
            _unitOfWork = unitOfWork;
            _maxLendingPeriodInDays = maxLendingPeriodInDays;
            _maxNumberOfBooksPerReader = maxNumberOfBooksPerReader;
        }

        public async Task<string> TakeBookAsync(string command)
        {
            var args = command.Split(' ');

            if (args.Length != 4)
                return "The command could not be processed.";

            int bookId;
            int readerId;
            short numberOfDays;

            try
            {
                bookId = int.Parse(args[1]);
                readerId = int.Parse(args[2]);
                numberOfDays = short.Parse(args[3]);
            }
            catch (Exception e)
            {
                return "The command could not be processed.";
            }

            var book = await _unitOfWork.Books.GetAsync(bookId);
            var reader = await _unitOfWork.Readers.GetAsync(readerId);

            if (book == null)
                return "The book was not found.";

            if (reader == null)
                return "The reader was not found.";
                                
            if (book.IsLent)
                return "The book could not be lent. It is already lent to another reader.";
            
            if (reader.TakenBooksCount >= _maxNumberOfBooksPerReader)
                return "The book could not be lent. The reader has already taken the permissible number of books.";
            
            if (numberOfDays > _maxLendingPeriodInDays || numberOfDays <= 0)
                return "The book could not be lent. Invalid lending period.";

            try
            {
                book.IsLent = true;
                reader.TakenBooksCount += 1;

                var lending = new Lending
                {
                    BookId = bookId,
                    ReaderId = readerId,
                    LendingDate = DateTime.Now,
                    LentUntil = DateTime.Now.AddDays(numberOfDays)
                };

                await _unitOfWork.Books.UpdateAsync(book);
                await _unitOfWork.Readers.UpdateAsync(reader);
                await _unitOfWork.Lendings.AddAsync(lending);
                
                return "The book was successfully lent.";
            }
            catch (Exception)
            {
                return "The book could not be lent. Unexpected exception occured.";
            }
        }

        public async Task<string> ReturnBookAsync(string command)
        {
            var args = command.Split(' ');

            if (args.Length != 2)
                return "The command could not be processed.";
            
            int bookId;

            try
            {
                bookId = int.Parse(args[1]);
            }
            catch (Exception e)
            {
                return "The command could not be processed.";
            }

            var lendings = await _unitOfWork.Lendings.GetAllAsync();
            var lending = lendings.SingleOrDefault(l => l.ReturnDate == null && l.BookId == bookId);

            if (lending == null)
                return "The lending was not found.";
            
            try
            {
                var reader = await _unitOfWork.Readers.GetAsync(lending.ReaderId);
                reader.TakenBooksCount -= 1;

                var book = await _unitOfWork.Books.GetAsync(bookId);
                book.IsLent = false;
                
                lending.ReturnDate = DateTime.Now;

                await _unitOfWork.Readers.UpdateAsync(reader);
                await _unitOfWork.Books.UpdateAsync(book);
                await _unitOfWork.Lendings.UpdateAsync(lending);

                if (lending.ReturnDate > lending.LentUntil)
                    return "Hey, you are returning the book too late! But we will forgive you this time :)\n\nThe book was successfully returned.";
                
                return "The book was successfully returned.";
            }
            catch (Exception)
            {
                return "The book could not be returned. Unexpected exception occured.";
            }
        }
    }
}