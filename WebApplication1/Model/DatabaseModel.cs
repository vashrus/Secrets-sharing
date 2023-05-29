using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Model;

public partial class DatabaseModel : DbContext
{
    public DatabaseModel()
    {
    }

    public DatabaseModel(DbContextOptions<DatabaseModel> options)
        : base(options)
    {
    }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Text> Texts { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<File>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.Files)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_File_User");
        });

        modelBuilder.Entity<Text>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.Texts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Text_User");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
