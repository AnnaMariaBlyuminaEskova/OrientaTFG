using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("Departments", Schema = "Master")]
public class Department
{
    /// <summary>
    /// Gets or sets the identifier
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the departament's name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the departament's tutors
    /// </summary>
    [InverseProperty(nameof(Department))]
    public virtual ICollection<Tutor>? Tutors { get; set; }
}
