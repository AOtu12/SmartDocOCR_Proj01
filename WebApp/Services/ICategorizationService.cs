using System.Threading.Tasks;

namespace WebApp.Services
{
    public interface ICategorizationService
    {
        Task<int?> PredictCategoryIdAsync(string extractedText);
    }
}
