using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos.Login
{
    public class VerifyCodeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Length(6, 6, ErrorMessage = "O código deve ter 6 dígitos.")]
        public string Code { get; set; } = null!;
    }
}