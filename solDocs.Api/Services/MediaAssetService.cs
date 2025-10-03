using solDocs.Models;
using MongoDB.Driver;
using solDocs.Interfaces;

namespace solDocs.Services
{
    public class MediaAssetService : IMediaAssetService
    {
        private readonly IMongoCollection<MediaAssetModel> _assetsCollection;

        public MediaAssetService(IMongoDatabase database)
        {
            _assetsCollection = database.GetCollection<MediaAssetModel>("MediaAssets");
        }

        public async Task<MediaAssetModel?> FindByHashAsync(string hash, string tenantId)
        {
            return await _assetsCollection.Find(a => a.FileHash == hash && a.TenantId == tenantId).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(MediaAssetModel asset)
        {
            asset.CreatedAt = DateTime.UtcNow;
            await _assetsCollection.InsertOneAsync(asset);
        }
    }
}