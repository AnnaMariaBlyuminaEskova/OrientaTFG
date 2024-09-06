using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("Students", Schema = "User")]
public class Student : User
{
    /// <summary>
    /// Gets or sets the student's tfg
    /// </summary>
    [InverseProperty(nameof(Student))]
    public virtual TFG? TFG { get; set; }

    /// <summary>
    /// Gets or sets the student's alert configuration
    /// </summary>
    [InverseProperty(nameof(Student))]
    public virtual StudentAlertConfiguration AlertConfiguration { get; set; } = null!;
}
