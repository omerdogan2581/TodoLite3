using Microsoft.EntityFrameworkCore;
using TodoLite.Models;

namespace TodoLite.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TodoItem> Todos { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
