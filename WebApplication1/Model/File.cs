using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Model;

[Table("File")]
public partial class File
{
    [Key]
    [Column("File_id")]
    public int FileId { get; set; }

    [StringLength(500)]
    public string Name { get; set; } = null!;

    [Column("isRemoved")]
    public bool IsRemoved { get; set; }

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Files")]
    public virtual User User { get; set; } = null!;
}
