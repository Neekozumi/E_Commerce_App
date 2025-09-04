namespace Afrodite.DTOs
{
    public class CreateRoleDto
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}