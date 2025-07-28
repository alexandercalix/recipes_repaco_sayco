using System;
using Microsoft.EntityFrameworkCore;
using RecipesRepacoSayco.Data.Models;

namespace RecipesRepacoSayco.Data.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<BatchProcess> BatchProcesses { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {


    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BatchProcess>(entity =>
        {
            entity.Property(e => e.Setpoint1).HasPrecision(10, 2);
            entity.Property(e => e.Setpoint2).HasPrecision(10, 2);
            entity.Property(e => e.Setpoint3).HasPrecision(10, 2);
            entity.Property(e => e.Setpoint4).HasPrecision(10, 2);
            entity.Property(e => e.Setpoint5).HasPrecision(10, 2);
            entity.Property(e => e.Setpoint6).HasPrecision(10, 2);

            entity.Property(e => e.ActualValue1).HasPrecision(10, 2);
            entity.Property(e => e.ActualValue2).HasPrecision(10, 2);
            entity.Property(e => e.ActualValue3).HasPrecision(10, 2);
            entity.Property(e => e.ActualValue4).HasPrecision(10, 2);
            entity.Property(e => e.ActualValue5).HasPrecision(10, 2);
            entity.Property(e => e.ActualValue6).HasPrecision(10, 2);
        });
    }

}