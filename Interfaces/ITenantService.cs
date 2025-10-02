using solDocs.Dtos;

namespace solDocs.Interfaces
{
    public interface ITenantService
    {
        Task<List<TenantResponseDto>> GetAllAsync();
        Task<TenantResponseDto?> GetByIdAsync(string id);
        Task<TenantResponseDto?> GetBySlugAsync(string slug);
        Task<TenantResponseDto?> GetByDominioAsync(string dominio);
        Task<TenantResponseDto> CreateAsync(CreateTenantDto dto);
        Task<TenantResponseDto?> UpdateAsync(string id, UpdateTenantDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> CheckLimiteUsuariosAsync(string tenantId, int usuariosAtuais);
        Task<bool> CheckLimiteArmazenamentoAsync(string tenantId, long armazenamentoAtual);
    }
}