using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("Administrator", Schema = "User")]
public class Administrator : BaseModel
{
    /// <summary>
    /// Gets or sets the the administrator's profile picture name
    /// </summary>
    [StringLength(300)]
    [Required]
    public string ProfilePictureName { get; set; }

    /// <summary>
    /// Gets or sets the administrator's email
    /// </summary>
    [StringLength(100)]
    [Required]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the administrator's password
    /// </summary>
    [StringLength(300)]
    [Required]
    public string Password { get; set; }
}
