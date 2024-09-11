using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nasa.Models;

namespace Nasa.Data
{
    public class ApplicationDbContext :IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
        {

        }
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<UserPost> posts { get; set; }
        public DbSet<AdminPost> adminPosts { get; set; }
        public DbSet<Message> messages { get; set; }
        public DbSet<UserFavoritePost> FavoritePosts { get; set; } // Added DbSet for the many-to-many relationship

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserFavoritePost>()
                .HasKey(e => new { e.PostId, e.AuthorId });

            
            modelBuilder.Entity<UserFavoritePost>()
         .HasOne(ufp => ufp.Post)
         .WithMany(p => p.UsersWhoFavorited)
         .HasForeignKey(ufp => ufp.PostId)
         .OnDelete(DeleteBehavior.Cascade); // or DeleteBehavior.SetNull or DeleteBehavior.Restrict

            modelBuilder.Entity<UserFavoritePost>()
                .HasOne(ufp => ufp.Author)
                .WithMany() // No navigation property in ApplicationUser for UserFavoritePost
                .HasForeignKey(ufp => ufp.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading to prevent cycles

            modelBuilder.Entity<UserPost>()
                .HasOne(up => up.Author)
                .WithMany(a => a.Posts) // Assuming ApplicationUser has a UserPosts collection
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading to prevent cycles

            modelBuilder.Entity<UserFavoritePost>()
                .Property(p => p.AuthorId)
                .HasMaxLength(450); // Example adjustment; modify as needed
        }


    }
}
