using Microsoft.EntityFrameworkCore;
using ProstirTgBot.Models;

namespace ProstirTgBot.Data
{
    internal class ProstirTgBotContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProstirTgBotDb;Persist Security Info=True;User ID=app_connection_login;Password=123456");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore("_day");
            modelBuilder.Ignore("_energy");
            modelBuilder.Ignore("_health");
            modelBuilder.Ignore("_happiness");
            modelBuilder.Ignore("_inGameName");
        }
    }

}
