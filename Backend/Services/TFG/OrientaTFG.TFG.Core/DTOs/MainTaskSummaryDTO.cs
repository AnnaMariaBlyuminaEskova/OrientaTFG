namespace OrientaTFG.TFG.Core.DTOs;

public class MainTaskSummaryDTO
{
    /// <summary>
    /// Gets or sets the main task's id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the main task's id
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the main task's order between all the tfg's tasks
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the main task's description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the main task's deadline
    /// </summary>
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the main task's maximum points
    /// </summary>
    public int MaximumPoints { get; set; }

    /// <summary>
    /// Gets or sets the main task's obtained points
    /// </summary>
    public int ObtainedPoints { get; set; }

    /// <summary>
    /// Gets or sets the count of sub tasks to do
    /// </summary>
    public int SubTasksToDo { get; set; }

    /// <summary>
    /// Gets or sets the count of hours of sub tasks to do
    /// </summary>
    public int HoursSubTasksToDo { get; set; }

    /// <summary>
    /// Gets or sets the count of done sub tasks
    /// </summary>
    public int DoneSubTasks { get; set; }

    /// <summary>
    /// Gets or sets the count of hours of done sub tasks
    /// </summary>
    public int HoursDoneSubTasks { get; set; }
}
