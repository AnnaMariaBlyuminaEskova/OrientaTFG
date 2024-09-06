namespace OrientaTFG.User.Core.DTOs;

public class ProfileDTO
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
    /// Gets or sets the user's email
    /// </summary>
    public string Email{ get; set; } = string.Empty;
}
