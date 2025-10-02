using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos
{
    public class CreateUpdateTopicDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "O tipo é obrigatório (publico ou privado)")]
        public string Type { get; set; } = null!;
        public string? ParentId { get; set; }

        public int Order { get; set; }
    }
}