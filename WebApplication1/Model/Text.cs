using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Model;

[Table("Text")]
public partial class Text
{
    [Key]
    [Column("Text_id")]
    public int TextId { get; set; }

    [StringLength(1000)]
    public string Content { get; set; } = null!;

    [Column("isRemoved")]
    public bool IsRemoved { get; set; }

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Texts")]
    public virtual User User { get; set; } = null!;
}
