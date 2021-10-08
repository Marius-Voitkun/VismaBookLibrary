using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using VismaBookLibrary.DAL;
using VismaBookLibrary.Models;
using VismaBookLibrary.Services;

namespace VismaBookLibrary
{
    public class CommandHandler
    {
        private readonly BooksService _booksService;
        private readonly ReadersService _readersService;
        private readonly LendingsService _lendingsService;

        public CommandHandler(IUnitOfWork unitOfWork, short maxLendingPeriodInDays, byte maxNumberOfBooksPerReader)
        {
            _booksService = new BooksService(unitOfWork);
            _readersService = new ReadersService(unitOfWork);
            _lendingsService = new LendingsService(unitOfWork, maxLendingPeriodInDays, maxNumberOfBooksPerReader);
        }

        public async Task<DataTransferHelper> IdentifyCommandAsync(string command)
        {
            switch (command.Split(' ')[0].ToLower())
            {
                case "add-book":
                    return new DataTransferHelper
                    {
                        NeedsAnswerCollecting = true,
                        Requests = GenerateMessagesForAddingBook()
                    };

                case "add-reader":
                    return new DataTransferHelper
                    {
                        NeedsAnswerCollecting = true,
                        Requests = GenerateMessagesForAddingReader()
                    };
                
                case "delete-book":
                    return new DataTransferHelper
                    {
                        Message = await _booksService.DeleteBookAsync(command)
                    };
                
                case "delete-reader":
                    return new DataTransferHelper
                    {
                        Message = await _readersService.DeleteReaderAsync(command)
                    };

                case "list-books":
                    return new DataTransferHelper
                    {
                        Message = await _booksService.GetBooksAsync()
                    };
                
                case "list-readers":
                    return new DataTransferHelper
                    {
                        Message = await _readersService.GetReadersAsync()
                    };

                case "return-book":
                    return new DataTransferHelper
                    {
                        Message = await _lendingsService.ReturnBookAsync(command)
                    };

                case "take-book":
                    return new DataTransferHelper
                    {
                        Message = await _lendingsService.TakeBookAsync(command)
                    };

                case "help":
                    return new DataTransferHelper
                    {
                        Message = GenerateHelpMessage()
                    };

                case "exit":
                    return new DataTransferHelper();

                default:
                    return new DataTransferHelper
                    {
                        Message = "The command was not recognized."
                    };
            }
        }

        public async Task<DataTransferHelper> ProcessAnswersAsync(List<string> answers)
        {
            switch (answers[0])
            {
                case "add-book":
                    return new DataTransferHelper
                    {
                        Message = await _booksService.AddNewBookAsync(answers)
                    };

                case "add-reader":
                    return new DataTransferHelper
                    {
                        Message = await _readersService.AddNewReaderAsync(answers)
                    };
                
                default:
                    return new DataTransferHelper();
            }
        }

        private List<string> GenerateMessagesForAddingBook()
        {
            return new List<string>
            {
                "Book name: ",
                "Author: ",
                "Category: ",
                "Language: ",
                "Publication date: ",
                "ISBN: "
            };
        }

        private List<string> GenerateMessagesForAddingReader()
        {
            return new List<string>
            {
                "First name: ",
                "Last name: ",
                "E-mail: ",
                "Phone number: "
            };
        }

        private string GenerateHelpMessage()
        {
            return 
@$"The following commands are available:
> add-book
> add-reader
> delete-book [bookId]
> delete-reader [readerId]
> list-books
> list-readers
> return-book [bookId]
> take-book [bookId] [readerId] [numberOfDays]
> help
> exit";
        }
    }
}
