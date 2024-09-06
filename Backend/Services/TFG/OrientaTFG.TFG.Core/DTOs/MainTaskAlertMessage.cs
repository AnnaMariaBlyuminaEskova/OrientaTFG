namespace OrientaTFG.TFG.Core.DTOs;

public class MainTaskAlertMessage : Message
{
    /// <summary>
    /// Gets or sets the main task's deadline
    /// </summary>
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Gets or sets the list of sub tasks to do
    /// </summary>
    public List<SubTaskMessage> SubTasksToDo { get; set; }
}
