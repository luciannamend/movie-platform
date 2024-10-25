using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace movie_platform.Models;

public partial class MovieplatformdbContext : DbContext
{
    public MovieplatformdbContext()
    {
    }

    public MovieplatformdbContext(DbContextOptions<MovieplatformdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=movieplatformdb.cd28kqsq05jg.us-east-1.rds.amazonaws.com,1433;database=movieplatformdb;User ID=movieadmin;Password=movieadmin;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3214EC07EB2C1BAC");

            entity.ToTable("user");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
