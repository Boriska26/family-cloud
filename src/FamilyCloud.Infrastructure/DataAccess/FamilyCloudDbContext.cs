using FamilyCloud.Domain;
using FamilyCloud.Infrastructure.DataAccess.Configurations;
using FamilyCloud.Infrastructure.DataAccess.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FamilyCloud.Infrastructure.DataAccess
{
    public class FamilyCloudDbContext : DbContext
    {
        public FamilyCloudDbContext(DbContextOptions<FamilyCloudDbContext> options) : base(options) { }

        public DbSet<Photo> Photos => Set<Photo>();

        public DbSet<PhotoFile> PhotoFiles => Set<PhotoFile>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public DbSet<User> Users => Set<User>();

        public DbSet<Person> Persons => Set<Person>();

        public DbSet<Invitation> Invitations => Set<Invitation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new InvitationConfiguration());
            modelBuilder.ApplyConfiguration(new PhotoConfiguration());
            modelBuilder.ApplyConfiguration(new PhotoFileConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
