using FamilyCloud.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCloud.Infrastructure.DataAccess.Configurations
{
    public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
    {
        public void Configure(EntityTypeBuilder<Invitation> builder)
        {
            builder.HasOne<User>().WithMany().HasForeignKey(n=>n.InvitedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Person>().WithMany().HasForeignKey(n=>n.PersonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(i => i.TokenHash).IsUnique();
        }
    }
}