using System.Threading.Tasks;

namespace WebApp.Services
{
    public interface IOcrService
    {
        Task<string> ExtractTextAsync(string imagePath);
    }
}
