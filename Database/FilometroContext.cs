using Microsoft.EntityFrameworkCore;

namespace FilometroWorker.Database
{
    public class FilometroContext : DbContext
    {
        public DbSet<Subscription> Subscriptions { get; set; }

  

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=10.0.1.154;database=filometro;user=root;password=princesa");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ChatId).IsRequired();
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.NomePosto).IsRequired();
            });

        }
    }
}
