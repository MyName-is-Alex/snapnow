using System.ComponentModel.DataAnnotations;

namespace snapnow.DTOS;

public class RoleModel
{
    [Required]
    public string RoleName { get; set; }
}