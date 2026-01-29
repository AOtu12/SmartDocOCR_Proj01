using System.Linq;                         
using System.Threading.Tasks;             
using Microsoft.EntityFrameworkCore;       
using WebApp.Data;                         

namespace WebApp.Services
{
    public class KeywordCategorizationService : ICategorizationService
    {
        private readonly ApplicationDbContext _db;   // Database context for retrieving category records

        public KeywordCategorizationService(ApplicationDbContext db) => _db = db;
        // Constructor injection assigns db context

        public async Task<int?> PredictCategoryIdAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;          // Return null if text is empty

            text = text.ToLowerInvariant();                            // Normalize text for case-insensitive matching

            //  Extended keywords for better auto-categorization
            var rules = new (string keyword, string category)[]         // Rule list: keyword + resulting category
            {
                ("invoice", "Invoice"), ("amount due", "Invoice"), ("total", "Invoice"),
                ("receipt", "Receipt"), ("paid", "Receipt"), ("purchase", "Receipt"),
                ("passport", "ID"), ("driver", "ID"), ("license", "ID"),
                ("application", "Form"), ("form", "Form"), ("registration", "Form"),
                ("dear", "Letter"), ("sincerely", "Letter"), ("regards", "Letter"), ("Transcript", "Results")
            };

            var match = rules.FirstOrDefault(r => text.Contains(r.keyword));
            // Find the first rule whose keyword appears in the text

            if (match.category == null) return null;                   // If no keyword matched, return null

            var cat = await _db.Categories
                               .FirstOrDefaultAsync(c => c.Name == match.category);
            // Query the database for a category with the matched name

            return cat?.Id;                                            // Return category ID or null if not found
        }
    }
}
