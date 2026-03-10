using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Infrastructure.Data.Configurations;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.ToTable("Meeting");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.StartDate)
            .IsRequired();

        builder.Property(m => m.EndDate)
            .IsRequired();

        builder.Property(m => m.Status)
            .IsRequired();

        builder.Property(m => m.AccessToken)
            .IsRequired();

        builder.HasIndex(m => m.AccessToken)
            .IsUnique();

        builder.HasOne(m => m.CreatedBy)
            .WithMany(u => u.CreatedMeetings)
            .HasForeignKey(m => m.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
