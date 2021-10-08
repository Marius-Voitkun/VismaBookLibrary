using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VismaBookLibrary.Models;

namespace VismaBookLibrary.DAL
{
    public class Repository<T> : IRepository<T> where T : IModel
    {
        private readonly string _dataPath;
        private readonly string _lastIdPath;

        public Repository(string dataPath, string lastIdPath)
        {
            _dataPath = dataPath;
            _lastIdPath = lastIdPath;
        }

        public async Task<List<T>> GetAllAsync()
        {
            if (!File.Exists(_dataPath))
            {
                await File.WriteAllTextAsync(_dataPath, "[]");
            }

            string json = await File.ReadAllTextAsync(_dataPath);

            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public async Task AddAsync(T entity)
        {
            var entities = await GetAllAsync();

            var highestId = entities.Count != 0 ? 
                               entities.Select(e => e.Id).Max() :
                               0;
            var latestId = await GetLatestIdAsync();
            entity.Id = Math.Max(highestId, latestId) + 1;
            await SaveLatestIdAsync(entity.Id);
            
            entities.Add(entity);

            await File.WriteAllTextAsync(_dataPath, JsonConvert.SerializeObject(entities));
        }

        public async Task DeleteAsync(int id)
        {
            var entities = await GetAllAsync();
            var entityToDelete = entities.SingleOrDefault(e => e.Id == id);

            if (entityToDelete == null)
                throw new ArgumentException("Entity not found.");

            entities.Remove(entityToDelete);
            
            await File.WriteAllTextAsync(_dataPath, JsonConvert.SerializeObject(entities));
        }

        public async Task UpdateAsync(T entity)
        {
            var entities = await GetAllAsync();

            entities.Remove(entities.SingleOrDefault(e => e.Id == entity.Id));
            entities.Add(entity);
            
            await File.WriteAllTextAsync(_dataPath, JsonConvert.SerializeObject(entities.OrderBy(e => e.Id)));
        }
        
        private async Task<int> GetLatestIdAsync()
        {
            try
            {
                return int.Parse(await File.ReadAllTextAsync(_lastIdPath));
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private async Task SaveLatestIdAsync(int id)
        {
            await File.WriteAllTextAsync(_lastIdPath, id.ToString());
        }
    }
}