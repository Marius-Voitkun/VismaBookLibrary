using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBookLibrary.DAL;
using VismaBookLibrary.Models;

namespace VismaBookLibrary.Services
{
    public class BooksService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BooksService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GetBooksAsync()
        {
            var books = await _unitOfWork.Books.GetAllAsync();

            var booksInfo = "";

            foreach (var book in books)
            {
                booksInfo += @$"
Id: {book.Id}
Name: {book.Name}
Author: {book.Author}
Category: {book.Category}
Language: {book.Language}
Publication date: {book.PublicationDate.ToShortDateString()}
ISBN: {book.ISBN}
Is lent: {book.IsLent}
";
            }

            return booksInfo;
        }
        
        public async Task<string> AddNewBookAsync(List<string> args)
        {
            try
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
                return "The book was successfully added.";
            }
            catch (Exception)
            {
                return "The book could not be added.";
            }
        }

        public async Task<string> DeleteBookAsync(string command)
        {
            int id = 0;
            
            try
            {
                id = int.Parse(command.Split(' ')[1]);
            }
            catch (Exception)
            {
                return "The book ID was not recognized.";
            }

            var book = await _unitOfWork.Books.GetAsync(id);

            if (book == null)
                return "The book with given ID does not exist.";

            if (book.IsLent == true)
                return "The book could not be deleted, as it is lent.";

            await _unitOfWork.Books.DeleteAsync(id);
            return "The book was successfully deleted.";
        }
    }
}