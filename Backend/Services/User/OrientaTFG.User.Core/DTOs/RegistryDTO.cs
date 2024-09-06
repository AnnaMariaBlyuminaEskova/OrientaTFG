namespace OrientaTFG.User.Core.DTOs;

public class RegistryDTO : LogInDTO
{
    /// <summary>
    /// Gets or sets the user's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's surname
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's profile picture
    /// </summary>
    public string ProfilePicture { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's profile picture name
    /// </summary>
    public string ProfilePictureName { get; set; } = string.Empty;
}
