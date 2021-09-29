using Microsoft.EntityFrameworkCore;

namespace GaspApp.Data
{
    public class GaspDbContext : DbContext
    {
        public GaspDbContext(DbContextOptions<GaspDbContext> options)
            : base(options)
        { }

    }
}
