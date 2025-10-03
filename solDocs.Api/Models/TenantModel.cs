using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace solDocs.Models
{
    public class TenantModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug deve conter apenas letras minúsculas, números e hífens")]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        public string? Dominio { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Telefone { get; set; }

        // Controle de Licença
        public bool Ativo { get; set; } = true;

        [Required]
        public DateTime VencimentoDaLicenca { get; set; }

        [Required]
        [MaxLength(50)]
        public string PlanoId { get; set; } = "basic";

        // Limites
        public int LimiteUsuarios { get; set; } = 5;
        public long LimiteArmazenamento { get; set; } = 1_073_741_824; // 1GB em bytes

        // Personalização
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Cor deve ser um código hex válido")]
        public string? CorPrimaria { get; set; }

        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Cor deve ser um código hex válido")]
        public string? CorSecundaria { get; set; }

        // Endereço (opcional)
        public EnderecoTenant? Endereco { get; set; }

        // Configurações customizadas
        public Dictionary<string, object>? Configuracoes { get; set; }

        // Auditoria
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        // Soft Delete
        public bool Deletado { get; set; } = false;
        public DateTime? DataDelecao { get; set; }
    }
}