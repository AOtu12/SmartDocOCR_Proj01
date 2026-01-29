using System.Collections.Generic;

namespace WebApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public ICollection<Document>? Documents { get; set; }
    }
}
