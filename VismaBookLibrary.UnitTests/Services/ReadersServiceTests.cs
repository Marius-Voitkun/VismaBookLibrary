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
    public class ReadersServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private ReadersService _readersService;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _readersService = new ReadersService(_mockUnitOfWork.Object);

            var mockReadersRepository = new Mock<IRepository<Reader>>();
            _mockUnitOfWork.SetupGet(u => u.Readers).Returns(mockReadersRepository.Object);
        }
        
        [Test]
        public async Task GetReadersAsync_WhenCalled_ReturnAllReaders()
        {
            var readers = new List<Reader>
            {
                new Reader { Id = 1, FirstName = "Reader 1", TakenBooksCount = 0 },
                new Reader { Id = 2, FirstName = "Reader 2", TakenBooksCount = 3 }
            };
            
            _mockUnitOfWork.Setup(u => u.Readers.GetAllAsync().Result).Returns(readers);

            var expectedResult = @"
Id: 1
First name: Reader 1
Last name: 
E-mail: 
Phone number: 
Number of taken books: 0

Id: 2
First name: Reader 2
Last name: 
E-mail: 
Phone number: 
Number of taken books: 3
";
            
            var result = await _readersService.GetReadersAsync();
            
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        
        [Test]
        public async Task AddNewReaderAsync_5ValidArgs_CallRepositoryAddMethod()
        {
            var args = new List<string>
            {
                "add-reader",
                "John",
                "Smith",
                "j.smith@gmail.com",
                "+37061234567"
            };

            await _readersService.AddNewReaderAsync(args);

            _mockUnitOfWork.Verify(u => u.Readers.AddAsync(It.IsAny<Reader>()), Times.Once);
        }
        
        [Test]
        [TestCase("add-reader", "", "Less than 5 arguments")]
        [TestCase("add-reader", "", "", "", "", "More than 5 arguments")]
        public void AddNewReaderAsync_NumberOfArgsDoesNotMatch_ThrowArgumentException(params string[] args)
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _readersService.AddNewReaderAsync(args.ToList()));
        }
        
        [Test]
        public async Task DeleteReaderAsync_ReaderDoesNotHaveTakenBooks_CallRepositoryDeleteMethod()
        {
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(1).Result)
                .Returns(new Reader { Id = 1, TakenBooksCount = 0 });

            await _readersService.DeleteReaderAsync("delete-reader 1");
            
            _mockUnitOfWork.Verify(u => u.Readers.DeleteAsync(1), Times.Once);
        }
        
        [Test]
        public async Task DeleteReaderAsync_ReaderHasTakenBooks_DoNotCallRepositoryDeleteMethod()
        {
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(1).Result)
                .Returns(new Reader { Id = 1, TakenBooksCount = 1 });

            await _readersService.DeleteReaderAsync("delete-reader 1");
            
            _mockUnitOfWork.Verify(u => u.Readers.DeleteAsync(1), Times.Never);
        }
        
        [Test]
        [TestCase("delete-reader 2")]
        [TestCase("delete-reader 0")]
        [TestCase("delete-reader a")]
        [TestCase("delete-reader")]
        [TestCase("delete-reader 1 1")]
        [TestCase("delete-reader 1 -")]
        public async Task DeleteReaderAsync_WrongIdOrInvalidCommand_DoNotCallRepositoryDeleteMethod(string command)
        {
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(1).Result)
                .Returns(new Reader { Id = 1, TakenBooksCount = 0 });
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(It.Is<int>(i => i != 1)))
                .Returns(Task.FromResult((Reader)null));
            
            await _readersService.DeleteReaderAsync(command);
            
            _mockUnitOfWork.Verify(u => u.Readers.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}