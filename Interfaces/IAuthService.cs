using solDocs.Dtos;

namespace solDocs.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> AuthenticateAsync(string email, string password);
        Task<bool> RequestPasswordResetAsync(string email); 
        Task<string?> VerifyResetCodeAsync(string email, string code);
        Task<bool> ResetPasswordAsync(string userId, string tenantId, string newPassword);
    }
}