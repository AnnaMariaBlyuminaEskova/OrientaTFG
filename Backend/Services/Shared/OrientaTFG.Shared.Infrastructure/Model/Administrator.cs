using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("Administrator", Schema = "User")]
public class Administrator : User
{
}
