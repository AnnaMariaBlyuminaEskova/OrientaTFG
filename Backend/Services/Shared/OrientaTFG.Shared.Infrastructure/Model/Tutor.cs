using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("Tutors", Schema = "User")]
public class Tutor : User
{
    /// <summary>
    /// Gets or sets the tutor's department id
    /// </summary>
    [Required]
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the tutor's department 
    /// </summary>
    [ForeignKey(nameof(DepartmentId))]
    [InverseProperty(nameof(Model.Department.Tutors))]
    public virtual Department Department { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tutor's tfgs
    /// </summary>
    [InverseProperty(nameof(Tutor))]
    public virtual ICollection<TFG>? TFGs { get; set; }
}
