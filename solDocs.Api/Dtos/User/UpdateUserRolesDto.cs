using System.ComponentModel.DataAnnotations;

namespace solDocs.Dtos.User
{
    public class UpdateUserRolesDto
    {
        [Required]
        public List<string> Roles { get; set; } = new List<string>();
    }
}