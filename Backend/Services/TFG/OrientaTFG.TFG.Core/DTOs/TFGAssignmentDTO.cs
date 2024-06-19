namespace OrientaTFG.TFG.Core.DTOs;

public class TFGAssignmentDTO
{
    /// <summary>
    /// Gets or sets the tfg's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tutor's id
    /// </summary>
    public int TutorId { get; set; }

    /// <summary>
    /// Gets or sets the student's id
    /// </summary>
    public int StudentId { get; set; }
}
