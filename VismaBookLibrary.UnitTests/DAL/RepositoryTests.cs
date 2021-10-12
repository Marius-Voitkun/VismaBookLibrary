using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using VismaBookLibrary.DAL;

namespace VismaBookLibrary.UnitTests.DAL
{
    [TestFixture]
    public class RepositoryTests
    {
        private string _workingDataPath = "../../../DAL/MockData/WorkingData.json";
        private string _initialDataPath = "../../../DAL/MockData/ProperData/InitialData.json";
        private string _dataAfterAddPath = "../../../DAL/MockData/ProperData/DataAfterAdd.json";
        private string _dataAfterDeletePath = "../../../DAL/MockData/ProperData/DataAfterDelete.json";
        private string _dataAfterUpdatePath = "../../../DAL/MockData/ProperData/DataAfterUpdate.json";
        
        private string _lastIdPath = "../../../DAL/MockData/LastId.txt";
        
        private Repository<MockEntity> _repository;

        [SetUp]
        public async Task SetUp()
        {
            _repository = new Repository<MockEntity>(_workingDataPath, _lastIdPath);

            var initialData = await File.ReadAllTextAsync(_initialDataPath);
            await File.WriteAllTextAsync(_workingDataPath, initialData);

            await File.WriteAllTextAsync(_lastIdPath, "10");
        }

        [Test]
        public async Task GetAllAsync_FileWithValidDataExists_ReturnListOfEntities()
        {
            var entities = new List<MockEntity>
            {
                new() {Id = 3, Name = "Name 3"},
                new() {Id = 5, Name = "Name 5"},
                new() {Id = 8, Name = "Name 8"}
            };
            
            var result = await _repository.GetAllAsync();
            
            // I can not assert that result == entities, so I write the following instead:
            Assert.That(JsonConvert.SerializeObject(result), Is.EqualTo(JsonConvert.SerializeObject(entities)));
        }
        
        [Test]
        public async Task GetAllAsync_NoFileWithData_ReturnEmptyList()
        {
            File.Delete(_workingDataPath);

            var result = await _repository.GetAllAsync();

            Assert.That(result, Is.Empty);
            Assert.That(result, Is.InstanceOf<List<MockEntity>>());
        }
        
        [Test]
        public async Task GetAllAsync_FileWithInvalidData_ReturnEmptyList()
        {
            await File.WriteAllTextAsync(_workingDataPath, "abc");

            var result = await _repository.GetAllAsync();

            Assert.That(result, Is.Empty);
            Assert.That(result, Is.InstanceOf<List<MockEntity>>());
        }

        [Test]
        public async Task GetAsync_EntityWithGivenIdExists_ReturnEntity()
        {
            var result = await _repository.GetAsync(5);
            
            Assert.That(result.Id, Is.EqualTo(5));
            Assert.That(result.Name, Is.EqualTo("Name 5"));
        }
        
        [Test]
        public async Task GetAsync_EntityWithGivenIdDoesNotExist_ReturnNull()
        {
            var result = await _repository.GetAsync(1);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddAsync_WhenCalled_AddTheEntity()
        {
            var entity = new MockEntity
            {
                Name = "Added name"
            };

            await _repository.AddAsync(entity);
            
            Assert.AreEqual(await File.ReadAllTextAsync(_dataAfterAddPath),
                            await File.ReadAllTextAsync(_workingDataPath));
        }
        
        [Test]
        [TestCase("10", 11)]
        [TestCase("2", 9)]
        [TestCase("", 9)]
        [TestCase("abc", 9)]
        public async Task AddAsync_WhenCalled_SetRightEntityId(string savedLastId, int expectedEntityId)
        {
            await File.WriteAllTextAsync(_lastIdPath, savedLastId);
            var entity = new MockEntity
            {
                Name = "Added name"
            };

            await _repository.AddAsync(entity);
            
            Assert.That(entity.Id == expectedEntityId);
        }

        [Test]
        public async Task DeleteAsync_EntityWithGivenIdExists_DeleteEntity()
        {
            await _repository.DeleteAsync(5);
            
            Assert.AreEqual(await File.ReadAllTextAsync(_dataAfterDeletePath),
                            await File.ReadAllTextAsync(_workingDataPath));
        }
        
        [Test]
        public void DeleteAsync_EntityWithGivenIdDoesNotExist_ThrowArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _repository.DeleteAsync(1));
        }

        [Test]
        public async Task UpdateAsync_EntityWithGivenIdExists_UpdateEntity()
        {
            var entity = new MockEntity
            {
                Id = 5,
                Name = "Updated name"
            };

            await _repository.UpdateAsync(entity);
            
            Assert.AreEqual(await File.ReadAllTextAsync(_dataAfterUpdatePath),
                            await File.ReadAllTextAsync(_workingDataPath));
        }
        
        [Test]
        public void UpdateAsync_EntityWithGivenIdDoesNotExist_ThrowArgumentException()
        {
            var entity = new MockEntity
            {
                Id = 1,
                Name = "Updated name"
            };
            
            Assert.ThrowsAsync<ArgumentException>(async () => await _repository.UpdateAsync(entity));
        }
    }
}