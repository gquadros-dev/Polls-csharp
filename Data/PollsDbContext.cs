using Microsoft.EntityFrameworkCore;
using POLLS.Models;

namespace POLLS.Data
{
    public class PollsDbContext : DbContext
    {
        public PollsDbContext(DbContextOptions<PollsDbContext> options) : base(options) { }

        // Mapeia as classes para tabelas do banco
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<Vote> Votes { get; set; }

        // Configurações dos Models
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Poll)
                .WithMany()
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}