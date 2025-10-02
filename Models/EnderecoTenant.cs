using System.ComponentModel.DataAnnotations;

namespace solDocs.Models
{ 
    public class EnderecoTenant
    {
        [MaxLength(200)]
        public string? Logradouro { get; set; }

        [MaxLength(20)]
        public string? Numero { get; set; }

        [MaxLength(100)]
        public string? Complemento { get; set; }

        [MaxLength(100)]
        public string? Bairro { get; set; }

        [MaxLength(100)]
        public string? Cidade { get; set; }

        [MaxLength(2)]
        public string? Estado { get; set; }

        [MaxLength(10)]
        public string? Cep { get; set; }

        [MaxLength(100)]
        public string? Pais { get; set; } = "Brasil";
    }
}