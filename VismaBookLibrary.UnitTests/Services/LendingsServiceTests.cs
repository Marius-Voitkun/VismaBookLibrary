using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VismaBookLibrary.DAL;
using VismaBookLibrary.Models;
using VismaBookLibrary.Services;

namespace VismaBookLibrary.UnitTests.Services
{
    [TestFixture]
    public class LendingsServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private LendingsService _lendingsService;
        private readonly short _maxLendingPeriodInDays = 60;
        private readonly byte _maxNumberOfBooksPerReader = 3;

        [SetUp]
        public void SetUp()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _lendingsService = new LendingsService(_mockUnitOfWork.Object, _maxLendingPeriodInDays, _maxNumberOfBooksPerReader);

            var mockBooksRepository = new Mock<IRepository<Book>>();
            var mockReadersRepository = new Mock<IRepository<Reader>>();
            var mockLendingsRepository = new Mock<IRepository<Lending>>();

            _mockUnitOfWork.SetupGet(u => u.Books).Returns(mockBooksRepository.Object);
            _mockUnitOfWork.SetupGet(u => u.Readers).Returns(mockReadersRepository.Object);
            _mockUnitOfWork.SetupGet(u => u.Lendings).Returns(mockLendingsRepository.Object);
        }

        [Test]
        public async Task TakeBookAsync_LendingAllowed_RecordLendingAndUpdateBookAndReader()
        {
            var book = new Book { Id = 1, IsLent = false };
            var reader = new Reader { Id = 1, TakenBooksCount = 0 };
            
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(1).Result).Returns(book);
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(1).Result).Returns(reader);
            
            string command = $"take-book 1 1 {_maxLendingPeriodInDays}";

            await _lendingsService.TakeBookAsync(command);
            
            Assert.That(book.IsLent == true);
            Assert.That(reader.TakenBooksCount == 1);
            _mockUnitOfWork.Verify(u => u.Books.UpdateAsync(book), Times.Once);
            _mockUnitOfWork.Verify(u => u.Readers.UpdateAsync(reader), Times.Once);
            _mockUnitOfWork.Verify(u => u.Lendings.AddAsync(It.IsAny<Lending>()), Times.Once);
        }

        [Test]
        [TestCase(true, 0, 5)]
        [TestCase(false, 3, 5)]
        [TestCase(false, 0, 61)]
        [TestCase(false, 0, 0)]
        public async Task TakeBookAsync_LendingNotAllowed_DoNotRecordLendingAndDoNotUpdateBookAndReader
            (bool isLent, byte takenBooksCount, short numberOfDays)
        {
            var book = new Book { Id = 1, IsLent = isLent };
            var reader = new Reader { Id = 1, TakenBooksCount = takenBooksCount };
            
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(1).Result).Returns(book);
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(1).Result).Returns(reader);
            
            string command = $"take-book 1 1 {numberOfDays}";

            await _lendingsService.TakeBookAsync(command);
            
            _mockUnitOfWork.Verify(u => u.Books.UpdateAsync(book), Times.Never);
            _mockUnitOfWork.Verify(u => u.Readers.UpdateAsync(reader), Times.Never);
            _mockUnitOfWork.Verify(u => u.Lendings.AddAsync(It.IsAny<Lending>()), Times.Never);
        }

        [Test]
        [TestCase("take-book")]
        [TestCase("take-book 1")]
        [TestCase("take-book 1 1")]
        [TestCase("take-book 1 1 5 1")]
        [TestCase("take-book 2 1 5")]
        [TestCase("take-book 1 2 5")]
        [TestCase("take-book 1 1 a")]
        [TestCase("take-book a 1 5")]
        [TestCase("take-book 1 a 5")]
        public async Task TakeBookAsync_WrongIdsOrInvalidCommand_DoNotRecordLendingAndDoNotUpdateBookAndReader(string command)
        {
            var book = new Book { Id = 1, IsLent = false };
            var reader = new Reader { Id = 1, TakenBooksCount = 0 };
            
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(1).Result).Returns(book);
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(It.Is<int>(i => i != 1)))
                .Returns(Task.FromResult((Book)null));
            
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(1).Result).Returns(reader);
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(It.Is<int>(i => i != 1)))
                .Returns(Task.FromResult((Reader)null));
            
            await _lendingsService.TakeBookAsync(command);
            
            _mockUnitOfWork.Verify(u => u.Books.UpdateAsync(book), Times.Never);
            _mockUnitOfWork.Verify(u => u.Readers.UpdateAsync(reader), Times.Never);
            _mockUnitOfWork.Verify(u => u.Lendings.AddAsync(It.IsAny<Lending>()), Times.Never);
        }

        [Test]
        public async Task ReturnBookAsync_IdRefersToLentBook_UpdateBooksReadersAndLendings()
        {
            var book = new Book { Id = 1, IsLent = true };
            var reader = new Reader { Id = 1, TakenBooksCount = 1 };
            var lending = new Lending { Id = 1, BookId = 1, ReaderId = 1 };
            var lendings = new List<Lending>
            {
                lending,
                new Lending { ReturnDate = new DateTime(2021, 10, 1) }
            };
            
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(1).Result).Returns(book);
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(1).Result).Returns(reader);
            _mockUnitOfWork.Setup(u => u.Lendings.GetAllAsync().Result).Returns(lendings);

            await _lendingsService.ReturnBookAsync("return-book 1");
            
            Assert.That(book.IsLent == false);
            Assert.That(reader.TakenBooksCount == 0);
            Assert.That(lending.ReturnDate, Is.Not.Null);
            _mockUnitOfWork.Verify(u => u.Books.UpdateAsync(book), Times.Once);
            _mockUnitOfWork.Verify(u => u.Readers.UpdateAsync(reader), Times.Once);
            _mockUnitOfWork.Verify(u => u.Lendings.UpdateAsync(lending), Times.Once);
        }

        [Test]
        [TestCase(false, "return-book 1")]
        [TestCase(true, "return-book")]
        [TestCase(true, "return-book 1 1")]
        [TestCase(true, "return-book a")]
        public async Task ReturnBookAsync_IdRefersToNotLentBookOrInvalidCommand_DoNotUpdateBooksReadersAndLendings
            (bool isLent, string command)
        {
            var book = new Book { Id = 1, IsLent = isLent };
            var reader = new Reader { Id = 1, TakenBooksCount = 1 };
            var lending = new Lending { Id = 1, BookId = 1, ReaderId = 1, ReturnDate = new DateTime(2020, 10, 1)};
            var lendings = new List<Lending> { lending };
            
            _mockUnitOfWork.Setup(u => u.Books.GetAsync(1).Result).Returns(book);
            _mockUnitOfWork.Setup(u => u.Readers.GetAsync(1).Result).Returns(reader);
            _mockUnitOfWork.Setup(u => u.Lendings.GetAllAsync().Result).Returns(lendings);

            await _lendingsService.ReturnBookAsync(command);
            
            _mockUnitOfWork.Verify(u => u.Books.UpdateAsync(book), Times.Never);
            _mockUnitOfWork.Verify(u => u.Readers.UpdateAsync(reader), Times.Never);
            _mockUnitOfWork.Verify(u => u.Lendings.UpdateAsync(lending), Times.Never);
        }
    }
}