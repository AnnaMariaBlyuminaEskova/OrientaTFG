namespace OrientaTFG.User.Core.DTOs;

public class UpdateProfileDTO
{
    /// <summary>
    /// Gets or sets the user's id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
