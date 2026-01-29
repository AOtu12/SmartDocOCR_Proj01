using System;                           
using System.IO;                        
using System.Linq;                      
using System.Threading.Tasks;           
using Tesseract;                        
using UglyToad.PdfPig;                  
using UglyToad.PdfPig.Content;          
using SixLabors.ImageSharp;             
using SixLabors.ImageSharp.Processing; 

namespace WebApp.Services
{
    public class TesseractOcrService : IOcrService
    {
        private readonly IWebHostEnvironment _env;   // Environment for accessing content root path

        public TesseractOcrService(IWebHostEnvironment env)
        {
            _env = env;                              // Inject hosting environment
        }

        public async Task<string> ExtractTextAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))  // Validate file path
                return "❌ Invalid file path.";

            var ext = Path.GetExtension(filePath).ToLowerInvariant();  // Get file extension
            string resultText = string.Empty;                           // Store OCR result

            try
            {
                //  Handle PDF extraction
                if (ext == ".pdf")                                      
                {
                    resultText = await ExtractFromPdfAsync(filePath);   
                    if (!string.IsNullOrWhiteSpace(resultText))        
                        return resultText;                              
                }

                // Handle image extraction (fallback)
                resultText = await ExtractFromImageAsync(filePath);     
                return string.IsNullOrWhiteSpace(resultText)
                    ? "⚠️ No readable text detected."                   
                    : resultText;                                       
            }
            catch (Exception ex)
            {
                return $"❌ OCR failed: {ex.Message}";                  // Catch all errors
            }
        }

        //  Extracts text directly from PDF pages
        private async Task<string> ExtractFromPdfAsync(string pdfPath)
        {
            try
            {
                using var pdf = PdfDocument.Open(pdfPath);              // Open PDF file
                var text = string.Join("\n\n", pdf.GetPages()
                                                  .Select(p => p.Text)); // Extract text from each page

                if (!string.IsNullOrWhiteSpace(text))                   // If PDF contains selectable text
                    return await Task.FromResult(text.Trim());          // Return extracted text
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PDF ERROR] Fallback to OCR: {ex.Message}");  // Log error
            }

            // Fallback: If PDF text layer missing (scanned image PDF)
            return await ExtractFromImageAsync(pdfPath);                // Use OCR for scanned PDFs
        }

        //  Extract text from image files (Tesseract OCR)
        private async Task<string> ExtractFromImageAsync(string imagePath)
        {
            try
            {
                var preprocessed = PreprocessImage(imagePath);          // Preprocess image for clarity

                var tessdata = Path.Combine(_env.ContentRootPath, "tessdata"); // Path to tessdata folder
                using var engine = new TesseractEngine(tessdata, "eng", EngineMode.Default); // Create OCR engine

                engine.SetVariable("tessedit_char_whitelist",
                    "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz:/-.,");
                // Restrict recognized characters to improve accuracy

                using var img = Pix.LoadFromFile(preprocessed);         // Load processed image into Tesseract
                using var page = engine.Process(img);                   // Process image to extract text
                var text = page.GetText();                              // Get extracted text

                return await Task.FromResult(text.Trim());              // Return trimmed text
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IMAGE OCR ERROR] {ex.Message}");   // Log image OCR error
                return string.Empty;                                    // Return empty on failure
            }
        }

        // Optional preprocessing for cleaner OCR results
        private string PreprocessImage(string path)
        {
            try
            {
                var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                // Create temporary file for processed image

                using var image = Image.Load(path);                     // Load original image
                image.Mutate(x => x.Grayscale()                        // Convert to grayscale
                                     .Contrast(1.2f)                   // Improve contrast
                                     .BinaryThreshold(0.5f));          // Convert to clean black-white image

                image.Save(temp);                                      // Save the processed image
                return temp;                                           // Return path to processed file
            }
            catch
            {
                return path;                                           // If preprocessing fails, return original image
            }
        }
    }
}
