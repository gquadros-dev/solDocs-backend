using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do email é inválido.")]
        public string Email { get; set; } = null!;
    }
}