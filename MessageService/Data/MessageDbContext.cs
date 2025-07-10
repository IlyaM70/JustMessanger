using MessageService.Models;
using Microsoft.EntityFrameworkCore;

namespace MessageService.Data
{
    public class MessageDbContext : DbContext
    {
        public MessageDbContext(DbContextOptions<MessageDbContext> opts) : base(opts) { }
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = "1", Username="Bob" },
                new User { Id = "2", Username="Charlie" },
                new User { Id = "3", Username = "Karla" }
            );
        }

    }
}
