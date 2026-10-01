using FamilyCloud.Domain;
using Microsoft.EntityFrameworkCore;

namespace FamilyCloud.Infrastructure.DataAccess
{
    public class FamilyCloudDbContext : DbContext
    {
        public FamilyCloudDbContext(DbContextOptions<FamilyCloudDbContext> options) : base(options) { }

        public DbSet<Photo> Photos => Set<Photo>();
    }
}
