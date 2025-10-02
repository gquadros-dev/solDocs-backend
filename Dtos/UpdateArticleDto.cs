using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos
{
    public class UpdateArticleDto
    {
        [Required(ErrorMessage = "O título é obrigatório")]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
    }
}