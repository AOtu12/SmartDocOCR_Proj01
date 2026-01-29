using System;                                         
using System.IO;                                      
using System.Linq;                                    
using System.Threading.Tasks;                         
using Microsoft.AspNetCore.Authorization;             
using Microsoft.AspNetCore.Identity;                  
using Microsoft.AspNetCore.Mvc;                       
using Microsoft.EntityFrameworkCore;                  
using WebApp.Data;                                    
using WebApp.Models;                                 
using WebApp.Services;                                

namespace WebApp.Controllers
{
    [Authorize]                                       // Requires login to access controller
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _db;     // Database access
        private readonly UserManager<IdentityUser> _userManager;  // Identity user manager
        private readonly IOcrService _ocr;             // Injected OCR service
        private readonly ICategorizationService _cat;  // Injected categorization service
        private readonly IWebHostEnvironment _env;     // Host environment for file paths

        public DocumentsController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            IOcrService ocr,
            ICategorizationService cat,
            IWebHostEnvironment env)
        {
            _db = db;                                
            _userManager = userManager;               
            _ocr = ocr;                                 
            _cat = cat;                                 
            _env = env;                                 
        }

        
        // INDEX — List all uploaded documents with filters
        public async Task<IActionResult> Index(string? kw, int? categoryId, DateTime? from, DateTime? to)
        {
            var userId = _userManager.GetUserId(User)!;        

            var query = _db.Documents
                .Include(d => d.Category)                        
                .Where(d => d.UserId == userId);                

            if (categoryId.HasValue)                             // Category filter
                query = query.Where(d => d.CategoryId == categoryId);

            if (from.HasValue)                                   // From date filter
                query = query.Where(d => d.UploadDate >= from.Value);

            if (to.HasValue)                                     // To date filter
                query = query.Where(d => d.UploadDate <= to.Value);

            if (!string.IsNullOrWhiteSpace(kw))                  // Keyword search filter
            {
                var textMatches = _db.DocumentTexts              // Search in extracted OCR text
                    .Where(t => t.ExtractedText.Contains(kw))
                    .Select(t => t.DocumentId);

                query = query.Where(d => d.FileName.Contains(kw) || textMatches.Contains(d.Id));
            }

            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync(); // For dropdown filters
            ViewBag.Kw = kw;                                    // Pass keyword filter back to view
            ViewBag.CategoryId = categoryId;                    // Pass selected category
            ViewBag.From = from;
            ViewBag.To = to;

            var list = await query.OrderByDescending(d => d.UploadDate).ToListAsync(); // Most recent first
            return View(list);                                  // Return result to view
        }

       
        // UPLOAD — Upload a new file and process OCR
        public IActionResult Upload() => View();                 // Show upload page

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)                // Validate file input
            {
                ModelState.AddModelError("", "Please choose a file.");
                return View();
            }

            var userId = _userManager.GetUserId(User)!;          // Logged-in user ID

            var uploadsRoot = Path.Combine(
                _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
                "uploads", userId);                             // Folder: /wwwroot/uploads/{userId}

            Directory.CreateDirectory(uploadsRoot);              // Ensure folder exists

            var ext = Path.GetExtension(file.FileName);          // File extension
            var storedName = $"{Guid.NewGuid():N}{ext}";         // Unique stored filename
            var path = Path.Combine(uploadsRoot, storedName);    // Absolute disk path

            using (var stream = System.IO.File.Create(path))     // Save uploaded file to disk
                await file.CopyToAsync(stream);

            var relPath = Path.Combine("/uploads", userId, storedName).Replace("\\", "/");
            // Relative web path for display

            var doc = new Document                              // Create document DB record
            {
                FileName = file.FileName,                       // Original name
                FilePath = relPath,                             // Path for accessing file
                UploadDate = DateTime.UtcNow,                   // Upload timestamp
                UserId = userId                                 // Owner
            };

            _db.Documents.Add(doc);                             // Add to database
            await _db.SaveChangesAsync();                       // Save first so ID is generated

            // OCR Extraction
            Console.WriteLine($"[DEBUG] Running OCR on: {path}");
            var text = await _ocr.ExtractTextAsync(path);       // Extract OCR text
            Console.WriteLine($"[DEBUG] OCR Output: {text}");

            _db.DocumentTexts.Add(new DocumentText              // Save extracted text
            {
                DocumentId = doc.Id,
                ExtractedText = text
            });

            // Categorization
            var catId = await _cat.PredictCategoryIdAsync(text); // Predict category from OCR text
            if (catId.HasValue)
                doc.CategoryId = catId;                          // Assign category if detected

            await _db.SaveChangesAsync();                        // Save text + category

            TempData["msg"] = "✅ Upload complete — OCR text extracted and categorized.";
            return RedirectToAction(nameof(Index));              // Redirect after upload
        }

        
        // DETAILS — Show a document + extracted OCR text
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;          // Current user ID

            var doc = await _db.Documents
                .Include(d => d.Category)                        // Load category info
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            // Ensure user owns this document

            if (doc == null) return NotFound();

            var text = await _db.DocumentTexts
                .FirstOrDefaultAsync(t => t.DocumentId == id);   // Load extracted OCR text

            ViewBag.Text = text?.ExtractedText ?? "(No text extracted)";
            return View(doc);                                     // Pass document to view
        }

     
        // EDIT — Update document category
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var doc = await _db.Documents
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            // Ensure user owns document

            if (doc == null) return NotFound();

            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync(); // Category dropdown
            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Document model)
        {
            var userId = _userManager.GetUserId(User)!;

            var doc = await _db.Documents
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            // Ensure ownership

            if (doc == null) return NotFound();

            doc.CategoryId = model.CategoryId;                   // Update category
            _db.Update(doc);                                     // Mark entity modified
            await _db.SaveChangesAsync();

            TempData["msg"] = "✅ Document updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        
        // DELETE — Remove a document
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            var doc = await _db.Documents
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
            // Ensure document belongs to user

            if (doc == null) return NotFound();

            _db.Documents.Remove(doc);                           // Remove document
            await _db.SaveChangesAsync();                        // Commit delete

            TempData["msg"] = "🗑️ Document deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
