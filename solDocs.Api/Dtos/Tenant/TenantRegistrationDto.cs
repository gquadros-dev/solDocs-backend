using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos.Tenant
{
    public class TenantRegistrationDto
    {
        [Required(ErrorMessage = "O nome do Tenant é obrigatório")]
        [MaxLength(200)]
        public string TenantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Slug do Tenant é obrigatório")]
        [MaxLength(100)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "O Slug deve conter apenas letras minúsculas, números e hífens")]
        public string TenantSlug { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome de usuário do Admin é obrigatório")]
        [MinLength(3)]
        public string AdminUsername { get; set; } = null!;

        [Required(ErrorMessage = "O Email do Admin é obrigatório")]
        [EmailAddress]
        public string AdminEmail { get; set; } = null!;
        
        [Required(ErrorMessage = "A Senha do Admin é obrigatória")]
        [MinLength(8)]
        public string AdminPassword { get; set; } = null!;
    }
}