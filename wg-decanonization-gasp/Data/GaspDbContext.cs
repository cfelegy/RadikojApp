using GaspApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GaspApp.Data
{
    public class GaspDbContext : DbContext
    {
        public GaspDbContext(DbContextOptions<GaspDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Article> Articles { get; set; }
    }
}
