using System;                                  
using System.ComponentModel.DataAnnotations;   

namespace WebApp.Models
{
    public class Document
    {
        public int Id { get; set; }             // Primary key for the Document table

        [Required]
        public string FileName { get; set; } = default!;
        // Original name of the uploaded file (e.g., "invoice.pdf")

        [Required]
        public string FilePath { get; set; } = default!;
        // Physical or virtual storage path (e.g., "/uploads/{userId}/{guid}.ext")

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        // Timestamp of when the document was uploaded (default: now, in UTC)

        // classification
        public int? CategoryId { get; set; }
        // Optional foreign key referencing a Category

        public Category? Category { get; set; }
        // Navigation property to Category (EF Core relationship)

        // ownership
        [Required]
        public string UserId { get; set; } = default!;
        // ID of the user who uploaded/owns this document (foreign key to Identity user)
    }
}
