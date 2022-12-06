using Microsoft.EntityFrameworkCore;
using APIServer.Entities;

namespace APIServer.Data
{
    public class APIServerContext : DbContext
    {
        public APIServerContext(DbContextOptions<APIServerContext> options)
            : base(options)
        {

        }

        public DbSet<User> User { get; set; } = default!;
    }
}

