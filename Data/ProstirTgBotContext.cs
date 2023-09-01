using Microsoft.EntityFrameworkCore;
using ProstirTgBot.Models;

namespace ProstirTgBot.Data
{
    internal class ProstirTgBotContext : DbContext
    {
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<InGameEvent> InGameEvents { get; set; } = null!;
        public DbSet<InGameEventChoice> InGameEventChoice { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(DotNetEnv.Env.GetString("CONNECTION_STRING"));
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
