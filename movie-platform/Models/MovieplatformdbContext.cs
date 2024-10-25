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
         => optionsBuilder.UseSqlServer("");

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
