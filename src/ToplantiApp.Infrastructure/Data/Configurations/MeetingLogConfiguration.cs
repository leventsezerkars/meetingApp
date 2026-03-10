using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Infrastructure.Data.Configurations;

public class MeetingLogConfiguration : IEntityTypeConfiguration<MeetingLog>
{
    public void Configure(EntityTypeBuilder<MeetingLog> builder)
    {
        builder.ToTable("MeetingLog");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.MeetingName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(l => l.LogData)
            .HasColumnType("nvarchar(max)");
    }
}
