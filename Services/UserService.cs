using solDocs.Interfaces;
using solDocs.Models;
using MongoDB.Driver;

namespace solDocs.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<UserModel> _usersCollection;

        public UserService(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<UserModel>("Users");
        }
        
        public async Task<IEnumerable<UserModel>> GetAllUsersAsync(string tenantId)
        {
            return await _usersCollection.Find(u => u.TenantId == tenantId).ToListAsync();
        }
        
        public async Task<UserModel?> GetUserByIdAsync(string id, string tenantId)
        {
            return await _usersCollection.Find(u => u.Id == id && u.TenantId == tenantId).FirstOrDefaultAsync();
        }
        
        public async Task<UserModel?> UpdateUserRolesAsync(string id, string tenantId, List<string> roles)
        {
            var filter = Builders<UserModel>.Filter.And(
                Builders<UserModel>.Filter.Eq(u => u.Id, id),
                Builders<UserModel>.Filter.Eq(u => u.TenantId, tenantId)
            );
            var update = Builders<UserModel>.Update.Set(u => u.Roles, roles);
            var options = new FindOneAndUpdateOptions<UserModel> { ReturnDocument = ReturnDocument.After };

            return await _usersCollection.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<UserModel?> GetUserByEmailAsync(string email)
        {
            return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<UserModel?> GetUserByUsernameAsync(string username)
        {
            return await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
        }
        
        public async Task<UserModel> CreateUserAsync(UserModel user, string password)
        {
            user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            
            var now = DateTime.UtcNow;
            user.CreatedAt = now;
            user.UpdatedAt = now;

            await _usersCollection.InsertOneAsync(user);
            return user;
        }

        public async Task<bool> UpdatePasswordAsync(string id, string newHashedPassword)
        {
            var updateDefinition = Builders<UserModel>.Update
                .Set(u => u.HashedPassword, newHashedPassword)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _usersCollection.UpdateOneAsync(u => u.Id == id, updateDefinition);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}