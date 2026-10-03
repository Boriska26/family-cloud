using FamilyCloud.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCloud.Infrastructure.DataAccess.Configurations
{
    internal class PhotoFileConfiguration : IEntityTypeConfiguration<PhotoFile>
    {
        public void Configure(EntityTypeBuilder<PhotoFile> builder)
        {
            builder.Property(n => n.Id).ValueGeneratedNever();
            builder.Property(n => n.StorageKey).IsRequired();
            builder.Property(n => n.MimeType).IsRequired();
            builder.Property(n => n.PhotoId).IsRequired();
            builder.Property(n => n.Size).IsRequired();
            builder.Property(n => n.Variant).IsRequired();
            builder.HasIndex(n => new { n.PhotoId, n.Variant }).IsUnique();
        }
    }
}
