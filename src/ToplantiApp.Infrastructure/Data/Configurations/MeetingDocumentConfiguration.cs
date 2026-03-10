using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Infrastructure.Data.Configurations;

public class MeetingDocumentConfiguration : IEntityTypeConfiguration<MeetingDocument>
{
    public void Configure(EntityTypeBuilder<MeetingDocument> builder)
    {
        builder.ToTable("MeetingDocument");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(d => d.Meeting)
            .WithMany(m => m.Documents)
            .HasForeignKey(d => d.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
