using FamilyCloud.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCloud.Infrastructure.DataAccess.Configurations
{
    internal class PhotoConfiguration : IEntityTypeConfiguration<Photo>
    {
        public void Configure(EntityTypeBuilder<Photo> builder)
        {
            builder.Property(n => n.SourceName).IsRequired();
            builder.Property(n => n.UploadedAt).IsRequired();
            builder.HasMany(n => n.Files).WithOne().HasForeignKey(n => n.PhotoId);
            builder.Navigation(n => n.Files).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
