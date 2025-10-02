using solDocs.Dtos;
using solDocs.Interfaces;
using solDocs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace solDocs.Controllers
{
    [ApiController]
    [Route("api/{tenantSlug}/users")]
    [Authorize]
    public class UsersController : BaseTenantController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService, ITenantService tenantService) : base(tenantService)
        {
            _userService = userService;
        }
        
        [HttpGet] 
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(IEnumerable<UserModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            if (TenantId == null) return Unauthorized();
            
            var users = await _userService.GetAllUsersAsync(TenantId);
            
            users.ToList().ForEach(u => u.HashedPassword = "");

            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserDto registerDto)
        {
            if (TenantId == null) return Unauthorized();
            
            var usersInTenant = (await _userService.GetAllUsersAsync(TenantId)).ToList();
            var tenant = await GetTenantAsync();
            if (tenant == null) return Unauthorized();
            
            if (usersInTenant.Count >= tenant.LimiteUsuarios)
            {
                return BadRequest(new { message = $"Limite de {tenant.LimiteUsuarios} usuários atingido para este plano." });
            }
            
            if (await _userService.GetUserByEmailAsync(registerDto.Email) != null)
            {
                return BadRequest(new { message = "Este email já está em uso em outra conta." });
            }
            if (await _userService.GetUserByUsernameAsync(registerDto.Username) != null)
            {
                return BadRequest(new { message = "Este nome de usuário já está em uso." });
            }

            var newUser = new UserModel
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Roles = new List<string> { "user" },
                TenantId = TenantId
            };

            var createdUser = await _userService.CreateUserAsync(newUser, registerDto.Password);
            createdUser.HashedPassword = "";

            var tenantSlug = (await GetTenantAsync())?.Slug;
            var routeValues = new { tenantSlug = tenantSlug };
            return CreatedAtAction("default", routeValues, createdUser);
        }
        
        [HttpPatch("{id}/roles")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] UpdateUserRolesDto rolesDto)
        {
            if (TenantId == null) return Unauthorized();
            
            var updatedUser = await _userService.UpdateUserRolesAsync(id, TenantId, rolesDto.Roles);

            if (updatedUser == null)
            {
                return NotFound(new { message = "Usuário não encontrado neste tenant." });
            }
    
            updatedUser.HashedPassword = ""; 
            return Ok(updatedUser); 
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMe()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null) return Unauthorized();
            
            var user = await _userService.GetUserByEmailAsync(userEmail);
            if (user == null) return NotFound();

            user.HashedPassword = "";
            return Ok(user);
        }
    }
}