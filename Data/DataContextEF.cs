using DotnetAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Data;

public class DataContextEF : DbContext
{
    private readonly string? _connectionString;

    public DataContextEF(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserSalary> UserSalary { get; set; }
    public virtual DbSet<UserJobInfo> UserJobInfo { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                _connectionString,
                optionsBuilder => optionsBuilder.EnableRetryOnFailure()
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("TutorialAppSchema");
        modelBuilder
            .Entity<User>()
            .ToTable("Users", "TutorialAppSchema")
            .HasKey(user => user.UserId);
        modelBuilder.Entity<UserSalary>().HasKey(userSalary => userSalary.UserId);
        modelBuilder.Entity<UserJobInfo>().HasKey(userJobInfo => userJobInfo.UserId);
    }
}
