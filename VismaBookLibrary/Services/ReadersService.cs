using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VismaBookLibrary.DAL;
using VismaBookLibrary.Models;

namespace VismaBookLibrary.Services
{
    public class ReadersService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReadersService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<string> GetReadersAsync()
        {
            var readers = await _unitOfWork.Readers.GetAllAsync();

            if (readers.Count == 0)
                return "No readers were found.";
            
            var readersInfo = "";

            foreach (var reader in readers)
            {
                readersInfo += @$"
Id: {reader.Id}
First name: {reader.FirstName}
Last name: {reader.LastName}
E-mail: {reader.Email}
Phone number: {reader.PhoneNo}
Number of taken books: {reader.TakenBooksCount}
";
            }

            return readersInfo;
        }
        
        public async Task<string> AddNewReaderAsync(List<string> args)
        {
            if (args.Count != 5)
                throw new ArgumentException("The number of arguments does not match");
            
            try
            {
                var reader = new Reader
                {
                    FirstName = args[1],
                    LastName = args[2],
                    Email = args[3],
                    PhoneNo = args[4]
                };

                await _unitOfWork.Readers.AddAsync(reader);
                return "The reader was successfully added.";
            }
            catch (Exception)
            {
                return "The reader could not be added.";
            }
        }
        
        public async Task<string> DeleteReaderAsync(string command)
        {
            var args = command.Split(' ');

            if (args.Length != 2)
                return "The command could not be processed.";
            
            int id = 0;
            
            try
            {
                id = int.Parse(command.Split(' ')[1]);
            }
            catch (Exception)
            {
                return "The reader ID was not recognized.";
            }

            var reader = await _unitOfWork.Readers.GetAsync(id);

            if (reader == null)
                return "The reader with given ID does not exist.";

            if (reader.TakenBooksCount > 0)
                return "The reader could not be deleted, as he/she has not returned one or more books.";

            await _unitOfWork.Readers.DeleteAsync(id);
            return "The reader was successfully deleted.";
        }
    }
}