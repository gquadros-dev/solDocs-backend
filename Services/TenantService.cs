using solDocs.Dtos.Tenant;
using solDocs.Models;
using MongoDB.Driver;
using solDocs.Interfaces;

namespace solDocs.Services
{
    public class TenantService : ITenantService
    {
        private readonly IMongoCollection<TenantModel> _tenants;

        public TenantService(IMongoDatabase database)
        {
            _tenants = database.GetCollection<TenantModel>("tenants");
            
            // Criar índices únicos
            var indexKeysSlug = Builders<TenantModel>.IndexKeys.Ascending(t => t.Slug);
            var indexOptionsSlug = new CreateIndexOptions { Unique = true };
            _tenants.Indexes.CreateOneAsync(new CreateIndexModel<TenantModel>(indexKeysSlug, indexOptionsSlug));

            var indexKeysEmail = Builders<TenantModel>.IndexKeys.Ascending(t => t.Email);
            var indexOptionsEmail = new CreateIndexOptions { Unique = true };
            _tenants.Indexes.CreateOneAsync(new CreateIndexModel<TenantModel>(indexKeysEmail, indexOptionsEmail));

            var indexKeysDominio = Builders<TenantModel>.IndexKeys.Ascending(t => t.Dominio);
            var indexOptionsDominio = new CreateIndexOptions { Unique = true, Sparse = true };
            _tenants.Indexes.CreateOneAsync(new CreateIndexModel<TenantModel>(indexKeysDominio, indexOptionsDominio));
        }

        public async Task<List<TenantResponseDto>> GetAllAsync()
        {
            var tenants = await _tenants
                .Find(t => !t.Deletado)
                .SortBy(t => t.Nome)
                .ToListAsync();

            return tenants.Select(TenantResponseDto.FromTenant).ToList();
        }

        public async Task<TenantResponseDto?> GetByIdAsync(string id)
        {
            var tenant = await _tenants
                .Find(t => t.Id == id && !t.Deletado)
                .FirstOrDefaultAsync();

            return tenant != null ? TenantResponseDto.FromTenant(tenant) : null;
        }

        public async Task<TenantResponseDto?> GetBySlugAsync(string slug)
        {
            var tenant = await _tenants
                .Find(t => t.Slug == slug.ToLower() && !t.Deletado)
                .FirstOrDefaultAsync();

            return tenant != null ? TenantResponseDto.FromTenant(tenant) : null;
        }

        public async Task<TenantResponseDto?> GetByDominioAsync(string dominio)
        {
            var tenant = await _tenants
                .Find(t => t.Dominio == dominio.ToLower() && !t.Deletado)
                .FirstOrDefaultAsync();

            return tenant != null ? TenantResponseDto.FromTenant(tenant) : null;
        }

        public async Task<TenantResponseDto> CreateAsync(CreateTenantDto dto)
        {
            // Verificar se slug já existe
            var existingSlug = await _tenants
                .Find(t => t.Slug == dto.Slug.ToLower())
                .FirstOrDefaultAsync();

            if (existingSlug != null)
                throw new InvalidOperationException("Slug já está em uso");

            // Verificar se email já existe
            var existingEmail = await _tenants
                .Find(t => t.Email == dto.Email.ToLower())
                .FirstOrDefaultAsync();

            if (existingEmail != null)
                throw new InvalidOperationException("Email já está em uso");

            // Verificar se domínio já existe (se fornecido)
            if (!string.IsNullOrWhiteSpace(dto.Dominio))
            {
                var existingDominio = await _tenants
                    .Find(t => t.Dominio == dto.Dominio.ToLower())
                    .FirstOrDefaultAsync();

                if (existingDominio != null)
                    throw new InvalidOperationException("Domínio já está em uso");
            }

            var tenant = new TenantModel
            {
                Nome = dto.Nome,
                Slug = dto.Slug.ToLower(),
                Email = dto.Email.ToLower(),
                LogoUrl = dto.LogoUrl,
                Dominio = dto.Dominio?.ToLower(),
                Telefone = dto.Telefone,
                VencimentoDaLicenca = dto.VencimentoDaLicenca,
                PlanoId = dto.PlanoId,
                LimiteUsuarios = dto.LimiteUsuarios,
                LimiteArmazenamento = dto.LimiteArmazenamento,
                CorPrimaria = dto.CorPrimaria,
                CorSecundaria = dto.CorSecundaria,
                Endereco = dto.Endereco != null ? new EnderecoTenant
                {
                    Logradouro = dto.Endereco.Logradouro,
                    Numero = dto.Endereco.Numero,
                    Complemento = dto.Endereco.Complemento,
                    Bairro = dto.Endereco.Bairro,
                    Cidade = dto.Endereco.Cidade,
                    Estado = dto.Endereco.Estado,
                    Cep = dto.Endereco.Cep,
                    Pais = dto.Endereco.Pais ?? "Brasil"
                } : null,
                Configuracoes = dto.Configuracoes,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            await _tenants.InsertOneAsync(tenant);
            return TenantResponseDto.FromTenant(tenant);
        }

        public async Task<TenantResponseDto?> UpdateAsync(string id, UpdateTenantDto dto)
        {
            var tenant = await _tenants
                .Find(t => t.Id == id && !t.Deletado)
                .FirstOrDefaultAsync();

            if (tenant == null)
                return null;

            // Verificar email duplicado (se está mudando)
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.ToLower() != tenant.Email)
            {
                var existingEmail = await _tenants
                    .Find(t => t.Email == dto.Email.ToLower() && t.Id != id)
                    .FirstOrDefaultAsync();

                if (existingEmail != null)
                    throw new InvalidOperationException("Email já está em uso");
            }

            // Verificar domínio duplicado (se está mudando)
            if (!string.IsNullOrWhiteSpace(dto.Dominio) && dto.Dominio.ToLower() != tenant.Dominio)
            {
                var existingDominio = await _tenants
                    .Find(t => t.Dominio == dto.Dominio.ToLower() && t.Id != id)
                    .FirstOrDefaultAsync();

                if (existingDominio != null)
                    throw new InvalidOperationException("Domínio já está em uso");
            }

            var update = Builders<TenantModel>.Update
                .Set(t => t.DataAtualizacao, DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(dto.Nome))
                update = update.Set(t => t.Nome, dto.Nome);

            if (!string.IsNullOrWhiteSpace(dto.Email))
                update = update.Set(t => t.Email, dto.Email.ToLower());

            if (dto.LogoUrl != null)
                update = update.Set(t => t.LogoUrl, dto.LogoUrl);

            if (dto.Dominio != null)
                update = update.Set(t => t.Dominio, dto.Dominio.ToLower());

            if (dto.Telefone != null)
                update = update.Set(t => t.Telefone, dto.Telefone);

            if (dto.Ativo.HasValue)
                update = update.Set(t => t.Ativo, dto.Ativo.Value);

            if (dto.VencimentoDaLicenca.HasValue)
                update = update.Set(t => t.VencimentoDaLicenca, dto.VencimentoDaLicenca.Value);

            if (!string.IsNullOrWhiteSpace(dto.PlanoId))
                update = update.Set(t => t.PlanoId, dto.PlanoId);

            if (dto.LimiteUsuarios.HasValue)
                update = update.Set(t => t.LimiteUsuarios, dto.LimiteUsuarios.Value);

            if (dto.LimiteArmazenamento.HasValue)
                update = update.Set(t => t.LimiteArmazenamento, dto.LimiteArmazenamento.Value);

            if (dto.CorPrimaria != null)
                update = update.Set(t => t.CorPrimaria, dto.CorPrimaria);

            if (dto.CorSecundaria != null)
                update = update.Set(t => t.CorSecundaria, dto.CorSecundaria);

            if (dto.Endereco != null)
            {
                update = update.Set(t => t.Endereco, new EnderecoTenant
                {
                    Logradouro = dto.Endereco.Logradouro,
                    Numero = dto.Endereco.Numero,
                    Complemento = dto.Endereco.Complemento,
                    Bairro = dto.Endereco.Bairro,
                    Cidade = dto.Endereco.Cidade,
                    Estado = dto.Endereco.Estado,
                    Cep = dto.Endereco.Cep,
                    Pais = dto.Endereco.Pais ?? "Brasil"
                });
            }

            if (dto.Configuracoes != null)
                update = update.Set(t => t.Configuracoes, dto.Configuracoes);

            var result = await _tenants.FindOneAndUpdateAsync<TenantModel>(
                t => t.Id == id && !t.Deletado,
                update,
                new FindOneAndUpdateOptions<TenantModel> { ReturnDocument = ReturnDocument.After }
            );

            return result != null ? TenantResponseDto.FromTenant(result) : null;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<TenantModel>.Update
                .Set(t => t.Deletado, true)
                .Set(t => t.DataDelecao, DateTime.UtcNow)
                .Set(t => t.Ativo, false);

            var result = await _tenants.UpdateOneAsync(
                t => t.Id == id && !t.Deletado,
                update
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> CheckLimiteUsuariosAsync(string tenantId, int usuariosAtuais)
        {
            var tenant = await _tenants
                .Find(t => t.Id == tenantId && !t.Deletado)
                .FirstOrDefaultAsync();

            if (tenant == null)
                return false;

            return usuariosAtuais < tenant.LimiteUsuarios;
        }

        public async Task<bool> CheckLimiteArmazenamentoAsync(string tenantId, long armazenamentoAtual)
        {
            var tenant = await _tenants
                .Find(t => t.Id == tenantId && !t.Deletado)
                .FirstOrDefaultAsync();

            if (tenant == null)
                return false;

            return armazenamentoAtual < tenant.LimiteArmazenamento;
        }
    }    
}
