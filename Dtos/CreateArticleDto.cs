using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos
{
    public class CreateArticleDto
    {
        [Required(ErrorMessage = "O título é obrigatório")]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ID do tópico é obrigatório")]
        public string TopicId { get; set; } = null!;
    }
}