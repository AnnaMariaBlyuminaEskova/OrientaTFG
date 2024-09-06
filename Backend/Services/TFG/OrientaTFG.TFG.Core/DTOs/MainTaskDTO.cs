namespace OrientaTFG.TFG.Core.DTOs;

public class MainTaskDTO : TaskDTO
{
    /// <summary>
    /// Gets or sets the main task's deadline
    /// </summary>
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Gets or sets the main task's maximum points
    /// </summary>
    public int MaximumPoints { get; set; }

    /// <summary>
    /// Gets or sets the main task's description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main task's obtained points
    /// </summary>
    public int ObtainedPoints { get; set; }

    /// <summary>
    /// Gets or sets the main task's subtasks
    /// </summary>
    public List<SubTaskDTO> SubTasks { get; set; } = [];
}
