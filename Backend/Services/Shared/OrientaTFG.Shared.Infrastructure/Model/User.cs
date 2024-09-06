using System.ComponentModel.DataAnnotations;

namespace OrientaTFG.Shared.Infrastructure.Model;

public class User : BaseModel
{
    /// <summary>
    /// Gets or sets the user's name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the user's surname
    /// </summary>
    [StringLength(50)]
    [Required]
    public string Surname { get; set; }

    /// <summary>
    /// Gets or sets the the user's profile picture name
    /// </summary>
    [StringLength(300)]
    [Required]
    public string ProfilePictureName { get; set; }

    /// <summary>
    /// Gets or sets the user's email
    /// </summary>
    [StringLength(100)]
    [Required]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's password
    /// </summary>
    [StringLength(300)]
    [Required]
    public string Password { get; set; }
}
