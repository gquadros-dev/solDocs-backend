using solDocs.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using solDocs.Models;
using MongoDB.Driver;
using solDocs.Dtos;

namespace solDocs.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<PasswordResetTokenModel> _resetTokensCollection;
        private readonly IEmailService _emailService;
        private readonly ITenantService _tenantService;

        public AuthService(IUserService userService, IConfiguration configuration, IMongoDatabase database, IEmailService emailService, ITenantService tenantService)
        {
            _userService = userService;
            _configuration = configuration;
            _resetTokensCollection = database.GetCollection<PasswordResetTokenModel>("PasswordResetTokens");
            _emailService = emailService;
            _tenantService = tenantService;
        }

        public async Task<LoginResponseDto?> AuthenticateAsync(string email, string password)
        {
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
            {
                return null;
            }
            
            var tenant = await _tenantService.GetByIdAsync(user.TenantId);
            if (tenant == null)
            {
                return null; 
            }

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                TenantSlug = tenant.Slug
            };
        }

        private string GenerateJwtToken(UserModel user, bool isPasswordResetToken = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim("tenantId", user.TenantId)
            };
            
            if (isPasswordResetToken)
            {
                claims.Add(new Claim("type", "password-reset"));
            }

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = isPasswordResetToken ? DateTime.UtcNow.AddMinutes(10) : DateTime.UtcNow.AddHours(4.5),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return true;

            var code = new Random().Next(100000, 999999).ToString();
            
            var resetToken = new PasswordResetTokenModel
            {
                Email = email,
                HashedCode = BCrypt.Net.BCrypt.HashPassword(code), 
                ExpiresAt = DateTime.UtcNow.AddMinutes(5) 
            };

            await _resetTokensCollection.DeleteManyAsync(t => t.Email == email);
            await _resetTokensCollection.InsertOneAsync(resetToken);
            
            await _emailService.SendPasswordResetEmailAsync(email, code);

            return true;
        }

        public async Task<string?> VerifyResetCodeAsync(string email, string code)
        {
            var tokenDoc = await _resetTokensCollection.Find(t => t.Email == email).FirstOrDefaultAsync();

            if (tokenDoc == null || tokenDoc.ExpiresAt < DateTime.UtcNow || !BCrypt.Net.BCrypt.Verify(code, tokenDoc.HashedCode))
            {
                return null;
            }

            await _resetTokensCollection.DeleteOneAsync(t => t.Id == tokenDoc.Id);

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return null; 

            return GenerateJwtToken(user, isPasswordResetToken: true);
        }

        public async Task<bool> ResetPasswordAsync(string userId, string tenantId, string newPassword)
        {
            var user = await _userService.GetUserByIdAsync(userId, tenantId);
            if (user == null) return false;

            var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
    
            return await _userService.UpdatePasswordAsync(userId, newHashedPassword);
        }
    }
}