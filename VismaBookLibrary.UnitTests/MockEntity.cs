using VismaBookLibrary.Models;

namespace VismaBookLibrary.UnitTests
{
    public class MockEntity : IModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}