using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Infrastructure.Data.Configurations;

public class MeetingParticipantConfiguration : IEntityTypeConfiguration<MeetingParticipant>
{
    public void Configure(EntityTypeBuilder<MeetingParticipant> builder)
    {
        builder.ToTable("MeetingParticipants");

        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(mp => mp.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(mp => mp.ParticipantType)
            .IsRequired();

        builder.HasOne(mp => mp.Meeting)
            .WithMany(m => m.Participants)
            .HasForeignKey(mp => mp.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mp => mp.User)
            .WithMany(u => u.Participations)
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(mp => new { mp.MeetingId, mp.Email })
            .IsUnique();
    }
}
