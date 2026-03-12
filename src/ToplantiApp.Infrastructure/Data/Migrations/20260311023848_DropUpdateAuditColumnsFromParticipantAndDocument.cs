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
            // Sütun varsa kaldır (idempotent); Docker/mevcut DB'de sütun önceden kaldırılmış olabilir
            DropColumnIfExists(migrationBuilder, "MeetingParticipant", "UpdatedAt");
            DropColumnIfExists(migrationBuilder, "MeetingParticipant", "UpdatedByUserId");
            DropColumnIfExists(migrationBuilder, "MeetingDocument", "UpdatedAt");
            DropColumnIfExists(migrationBuilder, "MeetingDocument", "UpdatedByUserId");
        }

        private static void DropColumnIfExists(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql($@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[{table}]') AND name = N'{column}'
                )
                BEGIN
                    DECLARE @constraint nvarchar(200);
                    SELECT @constraint = d.name
                    FROM sys.default_constraints d
                    INNER JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
                    WHERE d.parent_object_id = OBJECT_ID(N'[{table}]') AND c.name = N'{column}';
                    IF @constraint IS NOT NULL
                        EXEC(N'ALTER TABLE [{table}] DROP CONSTRAINT [' + @constraint + '];');
                    EXEC(N'ALTER TABLE [{table}] DROP COLUMN [{column}];');
                END;
            ");
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
