namespace OrientaTFG.TFG.Core.DTOs;

public class TFGDTO
{
    /// <summary>
    /// Gets or sets the tfg's id
    /// </summary>
    public int Id {  get; set; }

    /// <summary>
    /// Gets or sets the tfg student's name
    /// </summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tfg student's surname
    /// </summary>
    public string StudentSurname { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tfg student's profile picture
    /// </summary>
    public string StudentProfilePicture {  get; set; } 

    /// <summary>
    /// Gets or sets the tfg's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tfg's number of delayed main tasks
    /// </summary>
    public int DelayedMainTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of main tasks that are completed and not evaluated yet
    /// </summary>
    public int MainTasksNotEvaluated { get; set; }

    /// <summary>
    /// Gets or sets the number of main tasks that are in progress
    /// </summary>
    public int TasksInProgress { get; set; }

    /// <summary>
    /// Gets or sets the number of sub tasks that have not been completed
    /// </summary>
    public int SubTasksToDo { get; set; }
}
