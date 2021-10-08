using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBookLibrary.Models;

namespace VismaBookLibrary.DAL
{
    public interface IRepository<T> where T : IModel
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetAsync(int id);
        Task AddAsync(T entity);
        Task DeleteAsync(int id);
        Task UpdateAsync(T entity);
    }
}