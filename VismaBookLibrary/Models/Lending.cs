using System;

namespace VismaBookLibrary.Models
{
    public class Lending : IModel
    {
        public int Id { get; set; }
        
        public int BookId { get; set; }

        public int ReaderId { get; set; }

        public DateTime LendingDate { get; set; }

        public DateTime LentUntil { get; set; }

        public DateTime? ReturnDate { get; set; }
    }
}
