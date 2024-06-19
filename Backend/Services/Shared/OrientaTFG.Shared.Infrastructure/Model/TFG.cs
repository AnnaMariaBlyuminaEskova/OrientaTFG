using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("TFG", Schema = "TFG")]
public class TFG : BaseModel
{
    /// <summary>
    /// Gets or sets the TFG's name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tutor's id
    /// </summary>
    [Required]
    public int TutorId { get; set; }

    /// <summary>
    /// Gets or sets the student's id
    /// </summary>
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// Gets or sets the tutor
    /// </summary>
    [ForeignKey(nameof(TutorId))]
    [InverseProperty(nameof(Model.Tutor.TFGs))]
    public virtual Tutor Tutor { get; set; } = null!;

    /// <summary>
    /// Gets or sets the student
    /// </summary>
    [ForeignKey(nameof(StudentId))]
    [InverseProperty(nameof(Model.Student.TFG))]
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tfg's main tasks
    /// </summary>
    [InverseProperty(nameof(TFG))]
    public virtual ICollection<MainTask>? MainTasks { get; set; }
}
