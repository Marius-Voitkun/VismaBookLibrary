using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<string> GetBooksAsync(string command)
        {
            var books = await _unitOfWork.Books.GetAllAsync();

            var filteredBooks = FilterBooks(books, command);

            if (filteredBooks.Count == 0)
                return "No books with given criteria were found.";

            var booksInfo = "";

            foreach (var book in filteredBooks)
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

        private List<Book> FilterBooks(List<Book> books, string command)
        {
            var filteringCriteria = ExtractFilteringCriteria(command);

            if (filteringCriteria.Count == 0)
                return books;

            foreach (var criterion in filteringCriteria)
            {
                var property = criterion.Key.ToLower();
                var value = criterion.Value.ToLower();

                if (property == "taken")
                {
                    books = FilterBooksByAvailability(books, true);
                    continue;
                }

                if (property == "available")
                {
                    books = FilterBooksByAvailability(books, false);
                    continue;
                }

                property = char.ToUpper(property[0]) + property.Substring(1);
                if (property.ToLower() == "isbn")
                    property = property.ToUpper();

                try
                {
                    books = books.Where(b => ((string)b.GetType().GetProperty(property)
                                                        ?.GetValue(b)).ToLower().Contains(value))
                                                .ToList();
                }
                catch (Exception)
                {
                }
            }

            return books;
        }

        private Dictionary<string, string> ExtractFilteringCriteria(string command)
        {
            var args = command.Split(' ');

            var filteringCriteria = new Dictionary<string, string>();

            for (var i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("-f-"))
                {
                    var property = args[i].Split('-')[2];
                    var value = "";

                    // Handling situation when value for filtering is in double quotes:
                    if (i < args.Length - 1 && args[i + 1].StartsWith('"'))
                    {
                        for (var a = i + 1; a < args.Length; a++)
                        {
                            value += " " + args[a];

                            if (args[a].EndsWith('"'))
                            {
                                i = a;
                                value = value.Substring(2, value.Length - 3);
                                break;
                            };
                        }
                    }
                    // Handling situation when value for filtering is without quotes
                    else if (i < args.Length - 1 && !args[i + 1].StartsWith("-f-"))
                    {
                        value = args[i + 1];
                        i++;
                    }

                    filteringCriteria.Add(property, value);
                }
            }

            return filteringCriteria;
        }

        private List<Book> FilterBooksByAvailability(List<Book> books, bool isLent)
        {
            if (isLent == true)
                return books.Where(b => b.IsLent == true).ToList();

            return books.Where(b => b.IsLent == false).ToList();
        }

        public async Task<string> AddNewBookAsync(List<string> args)
        {
            if (args.Count != 7)
                throw new ArgumentException("The number of arguments does not match");

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
            var args = command.Split(' ');

            if (args.Length != 2)
                return "The command could not be processed.";

            int id = 0;

            try
            {
                id = int.Parse(args[1]);
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