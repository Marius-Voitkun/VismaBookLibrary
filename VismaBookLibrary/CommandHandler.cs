using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBookLibrary.DAL;
using VismaBookLibrary.Models;
using VismaBookLibrary.Services;

namespace VismaBookLibrary
{
    public class CommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<string> IdentifyCommand(string command)
        {
            switch (command.Split(' ')[0].ToLower())
            {
                case "add":
                    return GenerateMessagesForAddingBook();

                case "delete":
                    return null;

                case "list":
                    return null;

                case "take":
                    return null;

                case "return":
                    return null;

                case "help":
                    return null;

                case "exit":
                    return null;

                default:
                    return null;
            }
        }

        public async Task ProcessAnswersAsync(List<string> answers)
        {
            switch (answers[0])
            {
                case "add":
                    await AddNewBookAsync(answers);
                    break;

                default:
                    throw new ArgumentException("Command not recognized.");
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

        public async Task AddNewBookAsync(List<string> args)
        {
            var book = new Book
            {
                Name = args[1],
                Author = args[2],
                Category = args[3],
                Language = args[4],
                PublicationDate = DateTime.Parse(args[5]),
                ISBN = args[6]
            };

            await _unitOfWork.Books.AddAsync(book);
        }
    }
}
