using Microsoft.EntityFrameworkCore;
using Radikoj.Models;

namespace Radikoj.Data
{
    public class RadikojDbContext : DbContext
    {
        public RadikojDbContext(DbContextOptions<RadikojDbContext> options)
            : base(options)
        {
        }

        // Set all DbSet to null! to silence NRT warnings:
        // -> https://docs.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Article> Articles { get; set; } = null!;
        public DbSet<LocalizedItem> LocalizedItems { get; set; } = null!;
        public DbSet<Survey> Surveys { get; set; } = null!;
        public DbSet<SurveyItem> SurveyItems { get; set; } = null!;
        public DbSet<SurveyResponse> SurveyResponses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Survey>()
                .HasMany(x => x.Items)
                .WithOne();
        }
    }
}
