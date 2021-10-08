using System;

namespace VismaBookLibrary.Models
{
    public class Book : IModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Category { get; set; }

        public string Language { get; set; }

        public DateTime PublicationDate { get; set; }

        public string ISBN { get; set; }

        public bool IsLent { get; set; }
    }
}
