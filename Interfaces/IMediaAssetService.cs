using solDocs.Models;

namespace solDocs.Interfaces
{
    public interface IMediaAssetService
    {
        Task<MediaAssetModel?> FindByHashAsync(string hash, string tenantId);
        Task CreateAsync(MediaAssetModel asset);
    }
}