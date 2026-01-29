namespace WebApp.Models
{
    public class DocumentText
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public Document Document { get; set; } = default!;
        public string ExtractedText { get; set; } = string.Empty;
    }
}
