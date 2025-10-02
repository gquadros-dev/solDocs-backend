using System.Security.Claims;
using solDocs.Dtos;
using solDocs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace solDocs.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var loginResponse = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);

            if (loginResponse == null)
            {
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }

            return Ok(loginResponse);
        }
        
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.RequestPasswordResetAsync(dto.Email);
            return Ok(new { message = "Se um usuário com este email existir, um código de recuperação foi enviado." });
        }

        [HttpPost("verify-code")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
        {
            var resetToken = await _authService.VerifyResetCodeAsync(dto.Email, dto.Code);
            if (resetToken == null)
            {
                return BadRequest(new { message = "Código inválido ou expirado." });
            }
            return Ok(new { token = resetToken });
        }

        [HttpPost("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var tokenType = User.FindFirstValue("type");
            if (tokenType != "password-reset")
            {
                return Forbid();
            }
    
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tenantId = User.FindFirstValue("tenantId"); 
            
            if (userId == null || tenantId == null) return Unauthorized();
    
            var success = await _authService.ResetPasswordAsync(userId, tenantId, dto.NewPassword);
            
            if (!success) return BadRequest(new { message = "Não foi possível redefinir a senha." });

            return Ok(new { message = "Senha redefinida com sucesso." });
        }
    }
}