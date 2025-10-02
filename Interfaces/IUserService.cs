using solDocs.Models;

namespace solDocs.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserModel>> GetAllUsersAsync(string tenantId); 
        Task<UserModel?> GetUserByIdAsync(string id, string tenantId);
        Task<UserModel?> UpdateUserRolesAsync(string id, string tenantId, List<string> roles);
        
        Task<UserModel?> GetUserByEmailAsync(string email);
        Task<UserModel?> GetUserByUsernameAsync(string username);
        Task<UserModel> CreateUserAsync(UserModel user, string password);
        Task<bool> UpdatePasswordAsync(string id, string newHashedPassword);
    }
}