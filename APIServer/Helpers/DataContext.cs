using Microsoft.EntityFrameworkCore;
using APIServer.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using MySql.EntityFrameworkCore.Storage.Internal;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
using System.Reflection.Metadata;

namespace APIServer.Helpers;

public class DataContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<RefreshToken> RefreshToken { get; set; }
    public DbSet<DBFile> DBFile { get; set; }

    private readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    //DbContextOptionsBuilder
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        
        var connectionString = Configuration.GetConnectionString("MVCServerContext");
        options.UseMySQL(connectionString);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>().HasOne<User>(r => r.User).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.UserId).IsRequired();
    }

}

