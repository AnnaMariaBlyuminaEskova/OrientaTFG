using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("StudentsAlertConfigurations", Schema = "User")]
public class StudentAlertConfiguration : BaseModel
{
    /// <summary>
    /// Gets or sets the student's id
    /// </summary>
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// Gets or sets the student
    /// </summary>
    [ForeignKey(nameof(StudentId))]
    [InverseProperty(nameof(Model.Student.AlertConfiguration))]
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Gets or sets if the student is going to receive an email when an alert is generated
    /// </summary>
    [Required]
    public bool AlertEmail { get; set; } = false;

    /// <summary>
    /// Gets or sets if the student is going to receive an email when a task is evaluated
    /// </summary>
    [Required]
    public bool CalificationEmail { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of hours of the task 
    /// </summary>
    [Required]
    public int TotalTaskHours { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of days in anticipation with which an alert will be generated for those tasks that have fewer hours than total taks hours
    /// </summary>
    [Required]
    public int AnticipationDaysForFewerThanTotalTaskHoursTasks { get; set; } = 3;

    /// <summary>
    /// Gets or sets the number of days in anticipation with which an alert will be generated for those tasks that have more hours than total taks hours
    /// </summary>
    [Required]
    public int AnticipationDaysForMoreThanTotalTaskHoursTasks { get; set; } = 5;
}
