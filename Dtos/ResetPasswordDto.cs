using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos
{
    public class ResetPasswordDto
    {
        [Required]
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        public string NewPassword { get; set; } = null!;
    }
}