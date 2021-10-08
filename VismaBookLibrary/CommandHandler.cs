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

        public CommandHandler(IUnitOfWork unitOfWork)
        {
            _booksService = new BooksService(unitOfWork);
            _readersService = new ReadersService(unitOfWork);
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
                    return new DataTransferHelper();

                case "take-book":
                    return new DataTransferHelper();

                case "help":
                    return new DataTransferHelper();

                case "exit":
                    return new DataTransferHelper();

                default:
                    return new DataTransferHelper();
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
    }
}
