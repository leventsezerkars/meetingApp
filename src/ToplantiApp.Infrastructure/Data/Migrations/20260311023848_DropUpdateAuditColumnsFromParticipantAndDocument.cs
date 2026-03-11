using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToplantiApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropUpdateAuditColumnsFromParticipantAndDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "MeetingParticipant");
            migrationBuilder.DropColumn(name: "UpdatedByUserId", table: "MeetingParticipant");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "MeetingDocument");
            migrationBuilder.DropColumn(name: "UpdatedByUserId", table: "MeetingDocument");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MeetingParticipant",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "MeetingParticipant",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MeetingDocument",
                type: "datetime2",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "MeetingDocument",
                type: "int",
                nullable: true);
        }
    }
}
