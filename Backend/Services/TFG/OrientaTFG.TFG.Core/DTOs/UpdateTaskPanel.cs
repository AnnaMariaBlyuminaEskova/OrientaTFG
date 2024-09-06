namespace OrientaTFG.TFG.Core.DTOs;

public class UpdateTaskPanel
{
    /// <summary>
    /// Gets or sets the main task's id
    /// </summary>
    public int MainTaskId { get; set; }

    /// <summary>
    /// Gets or sets the new sub tasks
    /// </summary>
    public List<NewSubTaskDTO>? NewSubTasks { get; set; }

    /// <summary>
    /// Gets or sets the updated sub tasks
    /// </summary>
    public List<UpdateSubTaskDTO>? UpdatedSubTasks { get; set;}

    /// <summary>
    /// Gets or sets the deleted sub tasks ids
    /// </summary>
    public List<int>? DeletedSubTasksIds { get; set; }

    /// <summary>
    /// Gets or sets the obtained points
    /// </summary>
    public int? ObtainedPoints { get; set; }
}
