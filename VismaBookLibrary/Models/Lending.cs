using System;

namespace VismaBookLibrary.Models
{
    public class Lending
    {
        public int BookId { get; set; }

        public int ClientId { get; set; }

        public DateTime LendingDate { get; set; }

        public DateTime LentUntil { get; set; }

        public DateTime ReturnDate { get; set; }
    }
}
