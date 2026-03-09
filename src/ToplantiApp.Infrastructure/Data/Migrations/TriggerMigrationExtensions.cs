using Microsoft.EntityFrameworkCore;

namespace ToplantiApp.Infrastructure.Data.Migrations;

public static class TriggerMigrationExtensions
{
    public static void ApplyTriggerMigrations(this AppDbContext context)
    {
        var triggerSql = @"
            IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Meeting_AfterDelete')
            BEGIN
                EXEC('
                    CREATE TRIGGER trg_Meeting_AfterDelete
                    ON Meetings
                    AFTER DELETE
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        INSERT INTO MeetingLogs (MeetingId, MeetingName, DeletedBySystem, DeletedAt, LogData)
                        SELECT 
                            d.Id, 
                            d.Name, 
                            1, 
                            GETDATE(),
                            (SELECT d.Id, d.Name, d.Description, d.StartDate, d.EndDate, 
                                    d.Status, d.CancelledAt, d.AccessToken, 
                                    d.CreatedByUserId, d.CreatedAt
                             FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
                        FROM deleted d
                    END
                ')
            END";

        context.Database.ExecuteSqlRaw(triggerSql);
    }
}
