namespace OrientaTFG.TFG.Core.DTOs;

public class TaskDTO
{
    /// <summary>
    /// Gets or sets the task's id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the task's name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the task's estimated hours
    /// </summary>
    public int EstimatedHours { get; set; }

    /// <summary>
    /// Gets or sets the task's total hours
    /// </summary>
    public int? TotalHours { get; set; }

    /// <summary>
    /// Gets or sets the task's status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the task's order between all the tasks
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the name of the creator of the task
    /// </summary>
    public string CreatedBy { get; set; }
}
