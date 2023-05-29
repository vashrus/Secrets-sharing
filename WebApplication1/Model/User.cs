using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Model;

[Table("User")]
public partial class User
{
    [Key]
    [Column("User_id")]
    public int UserId { get; set; }

    [Column("User_FIO")]
    [StringLength(50)]
    public string UserFio { get; set; } = null!;

    [StringLength(50)]
    public string Password { get; set; } = null!;

    [StringLength(50)]
    public string Mail { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<File> Files { get; set; } = new List<File>();

    [InverseProperty("User")]
    public virtual ICollection<Text> Texts { get; set; } = new List<Text>();
}
