using FamilyCloud.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCloud.Infrastructure.DataAccess.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasOne<Person>().WithOne().HasForeignKey<User>(n => n.PersonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(n => n.InvitedByUser).WithMany().HasForeignKey(n=>n.InvitedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(n=>n.Login).IsUnique();
            builder.Property(u => u.Login).HasMaxLength(100);
            builder.HasIndex(u=>u.PersonId).IsUnique();
        }
    }
}