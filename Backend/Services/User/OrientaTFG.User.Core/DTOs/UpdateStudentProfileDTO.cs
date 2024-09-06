namespace OrientaTFG.User.Core.DTOs;

public class UpdateStudentProfileDTO : UpdateProfileDTO
{
    /// <summary>
    /// Gets or sets if the student is going to receive an email when an alert is generated
    /// </summary>
    public bool AlertEmail { get; set; }

    /// <summary>
    /// Gets or sets if the student is going to receive an email when a task is evaluated
    /// </summary>
    public bool CalificationEmail { get; set; }

    /// <summary>
    /// Gets or sets the number of hours of the task 
    /// </summary>
    public int TotalTaskHours { get; set; }

    /// <summary>
    /// Gets or sets the number of days in anticipation with which an alert will be generated for those tasks that have fewer hours than total taks hours
    /// </summary>
    public int AnticipationDaysForFewerThanTotalTaskHoursTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of days in anticipation with which an alert will be generated for those tasks that have more hours than total taks hours
    /// </summary>
    public int AnticipationDaysForMoreThanTotalTaskHoursTasks { get; set; }
}
