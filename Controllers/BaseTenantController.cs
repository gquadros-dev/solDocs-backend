using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using solDocs.Interfaces;
using solDocs.Dtos.Tenant;

namespace solDocs.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseTenantController : ControllerBase
    {
        protected readonly ITenantService _tenantService;
        private string? _tenantId;
        private TenantResponseDto? _tenant;

        protected BaseTenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Obtém o ID do Tenant a partir do token do usuário logado.
        /// Retorna null se o tenantId não for encontrado, o que deve resultar em Unauthorized.
        /// </summary>
        protected string? TenantId
        {
            get
            {
                if (_tenantId == null)
                {
                    _tenantId = User.FindFirstValue("tenantId");
                }
                return _tenantId;
            }
        }
        
        /// <summary>
        /// Obtém o objeto Tenant completo do usuário logado.
        /// Faz a busca no banco de dados na primeira vez que é acessado.
        /// </summary>
        protected async Task<TenantResponseDto?> GetTenantAsync()
        {
            if (_tenant == null && !string.IsNullOrEmpty(TenantId))
            {
                _tenant = await _tenantService.GetByIdAsync(TenantId);
            }
            return _tenant;
        }
    }
}