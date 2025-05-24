using MessageService.Models;
using Microsoft.EntityFrameworkCore;

namespace MessageService.Data
{
    public class MessageDbContext : DbContext
    {
        public MessageDbContext(DbContextOptions<MessageDbContext> opts) : base(opts) { }
        public DbSet<Message> Messages { get; set; }

    }
}
