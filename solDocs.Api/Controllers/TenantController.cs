using solDocs.Dtos.Tenant;
using solDocs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using solDocs.Models;

namespace solDocs.Controllers
{
    [ApiController]
    [Route("api/tenants")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly ILogger<TenantsController> _logger;

        public TenantsController(ITenantService tenantService, IUserService userService, ILogger<TenantsController> logger)
        {
            _tenantService = tenantService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Registra um novo tenant e seu primeiro usuário admin (público).
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TenantResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TenantResponseDto>> Register([FromBody] TenantRegistrationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userService.GetUserByEmailAsync(dto.AdminEmail) != null)
            {
                return BadRequest(new { message = "Este email já está em uso." });
            }
            if (await _userService.GetUserByUsernameAsync(dto.AdminUsername) != null)
            {
                return BadRequest(new { message = "Este nome de usuário já está em uso." });
            }

            TenantResponseDto newTenant = null;
            try
            {
                var createTenantDto = new CreateTenantDto
                {
                    Nome = dto.TenantName,
                    Slug = dto.TenantSlug,
                    Email = dto.AdminEmail, 
                    VencimentoDaLicenca = DateTime.UtcNow.AddDays(7),
                    PlanoId = "trial",
                    LimiteUsuarios = 3
                };

                newTenant = await _tenantService.CreateAsync(createTenantDto);

                var adminUser = new UserModel
                {
                    Username = dto.AdminUsername,
                    Email = dto.AdminEmail,
                    Roles = new List<string> { "admin", "user" },
                    TenantId = newTenant.Id
                };

                await _userService.CreateUserAsync(adminUser, dto.AdminPassword);

                return CreatedAtAction(nameof(GetBySlug), new { slug = newTenant.Slug }, newTenant);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processo de registro para o slug {Slug}", dto.TenantSlug);
                
                if (newTenant != null)
                {
                    _logger.LogInformation("Iniciando rollback: deletando tenant recém-criado com ID {TenantId}", newTenant.Id);
                    await _tenantService.DeleteAsync(newTenant.Id);
                }
                
                return StatusCode(500, new { message = "Ocorreu um erro inesperado durante o registro. Nenhuma alteração foi feita." });
            }
        }


        /// <summary>
        /// Lista todos os tenants (apenas super_admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "super_admin")]
        public async Task<ActionResult<List<TenantResponseDto>>> GetAll()
        {
            try
            {
                var tenants = await _tenantService.GetAllAsync();
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar tenants");
                return StatusCode(500, new { message = "Erro ao listar tenants" });
            }
        }

        /// <summary>
        /// Busca tenant por ID (apenas super_admin)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "super_admin")]
        public async Task<ActionResult<TenantResponseDto>> GetById(string id)
        {
            try
            {
                var tenant = await _tenantService.GetByIdAsync(id);

                if (tenant == null)
                    return NotFound(new { message = "Tenant não encontrado" });

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tenant {TenantId}", id);
                return StatusCode(500, new { message = "Erro ao buscar tenant" });
            }
        }

        /// <summary>
        /// Busca tenant por slug (público)
        /// </summary>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<TenantResponseDto>> GetBySlug(string slug)
        {
            try
            {
                var tenant = await _tenantService.GetBySlugAsync(slug);

                if (tenant == null)
                    return NotFound(new { message = "Tenant não encontrado" });

                return Ok(new
                {
                    tenant.Id,
                    tenant.Nome,
                    tenant.Slug,
                    tenant.LogoUrl,
                    tenant.CorPrimaria,
                    tenant.CorSecundaria,
                    tenant.Ativo,
                    tenant.LicencaVencida
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tenant por slug {Slug}", slug);
                return StatusCode(500, new { message = "Erro ao buscar tenant" });
            }
        }

        /// <summary>
        /// Busca tenant por domínio (público)
        /// </summary>
        [HttpGet("dominio/{dominio}")]
        [AllowAnonymous]
        public async Task<ActionResult<TenantResponseDto>> GetByDominio(string dominio)
        {
            try
            {
                var tenant = await _tenantService.GetByDominioAsync(dominio);

                if (tenant == null)
                    return NotFound(new { message = "Tenant não encontrado" });

                return Ok(new
                {
                    tenant.Id,
                    tenant.Nome,
                    tenant.Slug,
                    tenant.LogoUrl,
                    tenant.Dominio,
                    tenant.CorPrimaria,
                    tenant.CorSecundaria,
                    tenant.Ativo,
                    tenant.LicencaVencida
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tenant por domínio {Dominio}", dominio);
                return StatusCode(500, new { message = "Erro ao buscar tenant" });
            }
        }

        /// <summary>
        /// Cria um novo tenant (apenas super_admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "super_admin")]
        public async Task<ActionResult<TenantResponseDto>> Create([FromBody] CreateTenantDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenant = await _tenantService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tenant");
                return StatusCode(500, new { message = "Erro ao criar tenant" });
            }
        }

        /// <summary>
        /// Atualiza um tenant (apenas super_admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "super_admin")]
        public async Task<ActionResult<TenantResponseDto>> Update(string id, [FromBody] UpdateTenantDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenant = await _tenantService.UpdateAsync(id, dto);

                if (tenant == null)
                    return NotFound(new { message = "Tenant não encontrado" });

                return Ok(tenant);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tenant {TenantId}", id);
                return StatusCode(500, new { message = "Erro ao atualizar tenant" });
            }
        }

        /// <summary>
        /// Deleta um tenant (soft delete - apenas super_admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "super_admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _tenantService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Tenant não encontrado" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar tenant {TenantId}", id);
                return StatusCode(500, new { message = "Erro ao deletar tenant" });
            }
        }
    }
}