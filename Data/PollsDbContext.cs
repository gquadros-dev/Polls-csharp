using POLLS.Models;
using Microsoft.EntityFrameworkCore;

namespace POLLS.Data;

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
        modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.SessionId, v.PollId })
            .IsUnique();
    }
}