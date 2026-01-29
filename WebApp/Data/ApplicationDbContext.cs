using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentText> DocumentTexts => Set<DocumentText>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>().HasData(

                new Category { Id = 1, Name = "Invoice" },
                new Category { Id = 2, Name = "Receipt" },
                new Category { Id = 3, Name = "ID" },
                new Category { Id = 4, Name = "Letter" },
                new Category { Id = 5, Name = "Form" }
            );
        }
    }
}
