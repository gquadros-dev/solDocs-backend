using solDocs.Dtos.Tenant;
using solDocs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace solDocs.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantsController> _logger;

        public TenantsController(ITenantService tenantService, ILogger<TenantsController> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        /// <summary>
        /// Lista todos os tenants (apenas admin)
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
        /// Busca tenant por ID
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

                // Retornar apenas dados públicos
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
        /// Cria um novo tenant (apenas admin)
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
        /// Atualiza um tenant (apenas admin)
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
        /// Deleta um tenant (soft delete - apenas admin)
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