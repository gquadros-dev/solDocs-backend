using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos
{
    public class CreateTenantDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug é obrigatório")]
        [MaxLength(100)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug deve conter apenas letras minúsculas, números e hífens")]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        public string? Dominio { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "Data de vencimento é obrigatória")]
        public DateTime VencimentoDaLicenca { get; set; }

        [Required]
        [MaxLength(50)]
        public string PlanoId { get; set; } = "basic";

        public int LimiteUsuarios { get; set; } = 5;
        public long LimiteArmazenamento { get; set; } = 1_073_741_824; // 1GB

        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Cor primária deve ser um código hex válido")]
        public string? CorPrimaria { get; set; }

        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Cor secundária deve ser um código hex válido")]
        public string? CorSecundaria { get; set; }

        public EnderecoTenantDto? Endereco { get; set; }

        public Dictionary<string, object>? Configuracoes { get; set; }
    }   
}