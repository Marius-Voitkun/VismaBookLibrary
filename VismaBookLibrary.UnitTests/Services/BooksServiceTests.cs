using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VismaBookLibrary.DAL;
using VismaBookLibrary.Models;
using VismaBookLibrary.Services;

namespace VismaBookLibrary.UnitTests.Services
{
    [TestFixture]
    public class BooksServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private BooksService _booksService;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _booksService = new BooksService(_mockUnitOfWork.Object);

            var mockBooksRepository = new Mock<IRepository<Book>>();
            _mockUnitOfWork.SetupGet(u => u.Books).Returns(mockBooksRepository.Object);
        }

        [Test]
        public async Task GetBooksAsync_NoFiltering_ReturnAllBooks()
        {
            var books = new List<Book>
            {
                new Book{ Id = 1, Name = "Name 1", PublicationDate = new DateTime(2000, 01, 01) },
                new Book{ Id = 2, Name = "Name 2", PublicationDate = new DateTime(2020, 01, 01) }
            };
            
            _mockUnitOfWork.Setup(u => u.Books.GetAllAsync().Result).Returns(books);

            var expectedResult = @"
Id: 1
Name: Name 1
Author: 
Category: 
Language: 
Publication date: 2000-01-01
ISBN: 
Is lent: False

Id: 2
Name: Name 2
Author: 
Category: 
Language: 
Publication date: 2020-01-01
ISBN: 
Is lent: False
";
            
            var result = await _booksService.GetBooksAsync("list-books");
            
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task AddNewBookAsync_7ValidArgs_CallRepositoryAddMethod()
        {
            var args = new List<string>
            {
                "add-book",
                "Clean Code",
                "Robert C. Martin",
                "Programming",
                "English",
                "2009-03-01",
                "9780132350884"
            };

            await _booksService.AddNewBookAsync(args);

            _mockUnitOfWork.Verify(u => u.Books.AddAsync(It.IsAny<Book>()), Times.Once);
        }

        [Test]
        public async Task AddNewBookAsync_InvalidDateFormat_DoNotCallRepositoryAddMethod()
        {
            var args = new List<string> { "add-book", "", "", "", "", "Invalid date format", "" };
            
            await _booksService.AddNewBookAsync(args);

            _mockUnitOfWork.Verify(u => u.Books.AddAsync(It.IsAny<Book>()), Times.Never);
        }
        
        [Test]
        [TestCase("add-book", "", "Less than 7 arguments")]
        [TestCase("add-book", "", "", "", "", "2000-01-01", "", "More than 7 arguments, valid date format")]
        public void AddNewBookAsync_NumberOfArgsDoesNotMatch_ThrowArgumentException(params string[] args)
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _booksService.AddNewBookAsync(args.ToList()));
        }

        [Test]
        public async Task DeleteBookAsync_BookIsNotLent_CallRepositoryDeleteMethod()
        {
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(1).Result)
                .Returns(new Book { Id = 1, IsLent = false });

            await _booksService.DeleteBookAsync("delete-book 1");
            
            _mockUnitOfWork.Verify(u => u.Books.DeleteAsync(1), Times.Once);
        }

        [Test]
        public async Task DeleteBookAsync_BookIsLent_DoNotCallRepositoryDeleteMethod()
        {
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(1).Result)
                .Returns(new Book { Id = 1, IsLent = true });

            await _booksService.DeleteBookAsync("delete-book 1");
            
            _mockUnitOfWork.Verify(u => u.Books.DeleteAsync(1), Times.Never);
        }

        [Test]
        [TestCase("delete-book 2")]
        [TestCase("delete-book 0")]
        [TestCase("delete-book a")]
        [TestCase("delete-book")]
        [TestCase("delete-book 1 1")]
        [TestCase("delete-book 1 -")]
        public async Task DeleteBookAsync_WrongIdOrInvalidCommand_DoNotCallRepositoryDeleteMethod(string command)
        {
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(1).Result)
                .Returns(new Book { Id = 1, IsLent = false });
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(It.Is<int>(i => i != 1)))
                .Returns(Task.FromResult((Book)null));
            
            await _booksService.DeleteBookAsync(command);
            
            _mockUnitOfWork.Verify(u => u.Books.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}