using System.Security.Claims;

namespace Afrodite.DTOs
{
    public class EditUserDto 
    {
        public EditUserDto()
        {
            Claims = new List<string>();
            Roles = new List<string>();
        }

        [Required]
        public string Id { get; set; }
        public string FullName { get; set;}
        [Required]
        [EmailAddress]
        public string Email { get; set;}
        public string Address { get; set; }
        public List<string> Claims { get; set; }
        public IList<string> Roles { get; set; }

    }
}