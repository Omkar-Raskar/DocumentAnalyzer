using DocumentAnalyzer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace DocumentAnalyzer.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }
    }
}
