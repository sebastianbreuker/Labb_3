using System.Collections.Generic;
using System.Threading.Tasks;
using Labb_3.Models;

namespace Labb_3.Services
{    
    public interface IStorageService
    {
        Task<IReadOnlyList<QuestionPack>> LoadPacksAsync();
        Task SavePacksAsync(IEnumerable<QuestionPack> packs);

        Task<QuestionPack?> LoadPackFromFileAsync(string filePath);
        Task SavePackToFileAsync(QuestionPack pack, string filePath);
    }
}
