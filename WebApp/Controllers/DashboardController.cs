using Microsoft.AspNetCore.Authorization;                 
using Microsoft.AspNetCore.Identity;                      
using Microsoft.AspNetCore.Mvc;                           
using Microsoft.EntityFrameworkCore;                      
using System;
using System.Linq;                                        
using System.Threading.Tasks;                             
using WebApp.Data;                                        

namespace WebApp.Controllers
{
    [Authorize]                                           // Ensures only logged-in users can access this controller
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;         // Database context
        private readonly UserManager<IdentityUser> _userManager; // Identity user manager for current user id

        public DashboardController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;                                      // Assign injected DbContext
            _userManager = userManager;                    // Assign injected UserManager
        }

        // Dashboard main page
        public IActionResult Index() => View();            // Returns dashboard view

        // Uploads by Month (stacked bar chart)
        [HttpGet]
        public async Task<IActionResult> UploadsByMonth()
        {
            var userId = _userManager.GetUserId(User)!;    // Get logged-in user's ID

            // Step 1: group per year/month for both recognized and unrecognized
            var grouped = await _db.Documents
                .Where(d => d.UserId == userId && d.UploadDate != null)      // Filter documents for user
                .GroupBy(d => new
                {
                    d.UploadDate.Year,                                        
                    d.UploadDate.Month,                                       
                    IsRecognized = d.CategoryId != null                      
                })
                .Select(g => new
                {
                    g.Key.Year,                                               
                    g.Key.Month,                                              
                    g.Key.IsRecognized,                                       
                    Count = g.Count()                                         
                })
                .OrderBy(g => g.Year)                                        
                .ThenBy(g => g.Month)                                         
                .ToListAsync();                                               // Execute query

            // Step 2: shape data into chart format
            var labels = grouped
                .Select(g => $"{g.Year}-{g.Month:D2}")                        // Create "YYYY-MM" labels
                .Distinct()                                                   // Remove duplicates
                .ToList();

            var recognized = labels
                .Select(label =>
                {
                    var parts = label.Split('-');                             // Split year-month
                    var year = int.Parse(parts[0]);
                    var month = int.Parse(parts[1]);

                    return grouped.FirstOrDefault(g =>
                        g.Year == year &&
                        g.Month == month &&
                        g.IsRecognized)?.Count ?? 0;                          // Count recognized docs
                })
                .ToList();

            var unrecognized = labels
                .Select(label =>
                {
                    var parts = label.Split('-');                             // Split year-month
                    var year = int.Parse(parts[0]);
                    var month = int.Parse(parts[1]);

                    return grouped.FirstOrDefault(g =>
                        g.Year == year &&
                        g.Month == month &&
                        !g.IsRecognized)?.Count ?? 0;                         // Count unrecognized docs
                })
                .ToList();

            return Json(new { labels, recognized, unrecognized });            // Return chart data as JSON
        }

        //  Top Categories (Pie Chart)
        [HttpGet]
        public async Task<IActionResult> TopCategories()
        {
            var userId = _userManager.GetUserId(User)!;                       // Logged-in user ID

            var data = await _db.Documents
                .Where(d => d.UserId == userId)                               // Filter documents
                .Include(d => d.Category)                                     // Include category relationship
                .GroupBy(d => d.Category != null ? d.Category.Name : "Unrecognized")
                // Group by actual category name or "Unrecognized" if none

                .Select(g => new
                {
                    label = g.Key,                                            // Category name (label)
                    count = g.Count()                                         // Number of docs
                })
                .OrderByDescending(g => g.count)                              // Highest count first
                .Take(5)                                                     // Return top 5 categories
                .ToListAsync();                                               // Execute query

            return Json(data);                                                // Return pie chart data as JSON
        }
    }
}
