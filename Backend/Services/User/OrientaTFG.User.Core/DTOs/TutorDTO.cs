namespace OrientaTFG.User.Core.DTOs;

public class TutorDTO : UserDTO
{
    /// <summary>
    /// Gets or sets the tutor's departament name
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tutor's email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tutor's TFGs names
    /// </summary>
    public List<string> TFGs { get; set; } = new();
}
