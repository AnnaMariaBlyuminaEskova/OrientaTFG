﻿using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("Students", Schema = "User")]
public class Student : User
{
    /// <summary>
    /// Gets or sets the student's tfg
    /// </summary>
    [InverseProperty(nameof(Student))]
    public virtual TFG? TFG { get; set; }
}
