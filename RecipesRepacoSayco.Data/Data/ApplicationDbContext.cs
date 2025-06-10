using System;
using Microsoft.EntityFrameworkCore;
using RecipesRepacoSayco.Data.Models;

namespace RecipesRepacoSayco.Data.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<BatchProcess> BatchProcesses { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}