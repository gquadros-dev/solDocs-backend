using solDocs.Models;

namespace solDocs.Dtos.Tenant
{
    public class TenantResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Dominio { get; set; }
        public string? Telefone { get; set; }
        public bool Ativo { get; set; }
        public DateTime VencimentoDaLicenca { get; set; }
        public bool LicencaVencida => DateTime.UtcNow > VencimentoDaLicenca;
        public string PlanoId { get; set; } = string.Empty;
        public int LimiteUsuarios { get; set; }
        public long LimiteArmazenamento { get; set; }
        public string? CorPrimaria { get; set; }
        public string? CorSecundaria { get; set; }
        public EnderecoTenant? Endereco { get; set; }
        public Dictionary<string, object>? Configuracoes { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }

        public static TenantResponseDto FromTenant(TenantModel tenant)
        {
            return new TenantResponseDto
            {
                Id = tenant.Id,
                Nome = tenant.Nome,
                Slug = tenant.Slug,
                Email = tenant.Email,
                LogoUrl = tenant.LogoUrl,
                Dominio = tenant.Dominio,
                Telefone = tenant.Telefone,
                Ativo = tenant.Ativo,
                VencimentoDaLicenca = tenant.VencimentoDaLicenca,
                PlanoId = tenant.PlanoId,
                LimiteUsuarios = tenant.LimiteUsuarios,
                LimiteArmazenamento = tenant.LimiteArmazenamento,
                CorPrimaria = tenant.CorPrimaria,
                CorSecundaria = tenant.CorSecundaria,
                Endereco = tenant.Endereco,
                Configuracoes = tenant.Configuracoes,
                DataCriacao = tenant.DataCriacao,
                DataAtualizacao = tenant.DataAtualizacao
            };
        }
    }
}