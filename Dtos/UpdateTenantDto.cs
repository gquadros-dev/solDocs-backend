using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos
{
    public class UpdateTenantDto
    {
        [MaxLength(200)]
        public string? Nome { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        public string? Dominio { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Telefone { get; set; }

        public bool? Ativo { get; set; }

        public DateTime? VencimentoDaLicenca { get; set; }

        [MaxLength(50)]
        public string? PlanoId { get; set; }

        public int? LimiteUsuarios { get; set; }
        public long? LimiteArmazenamento { get; set; }

        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Cor primária deve ser um código hex válido")]
        public string? CorPrimaria { get; set; }

        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Cor secundária deve ser um código hex válido")]
        public string? CorSecundaria { get; set; }

        public EnderecoTenantDto? Endereco { get; set; }

        public Dictionary<string, object>? Configuracoes { get; set; }
    }
}   