using Microsoft.EntityFrameworkCore;

namespace Persistence.Seeders;

public static class TriggerSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        // 1. Trigger: TR_Assets_UpdateDepartmentAssetCount
        var triggerUpdateDepartmentAssetCount = @"
            IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = N'TR_Assets_UpdateDepartmentAssetCount')
            BEGIN
                EXEC dbo.sp_executesql @statement = N'
                CREATE TRIGGER [dbo].[TR_Assets_UpdateDepartmentAssetCount]
                ON [dbo].[Assets]
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    BEGIN TRY
                        ;WITH Changes AS
                        (
                            SELECT
                                DepartmentId,
                                COUNT(*) AS ChangeCount
                            FROM inserted
                            WHERE DepartmentId IS NOT NULL
                            GROUP BY DepartmentId

                            UNION ALL

                            SELECT
                                DepartmentId,
                                -COUNT(*) AS ChangeCount
                            FROM deleted
                            WHERE DepartmentId IS NOT NULL
                            GROUP BY DepartmentId
                        ),
                        DepartmentChanges AS
                        (
                            SELECT
                                DepartmentId,
                                SUM(ChangeCount) AS ChangeCount
                            FROM Changes
                            GROUP BY DepartmentId
                        )
                        UPDATE d
                        SET d.AssetCount = d.AssetCount + dc.ChangeCount
                        FROM dbo.Departments AS d
                        INNER JOIN DepartmentChanges AS dc
                            ON dc.DepartmentId = d.Id
                        WHERE dc.ChangeCount <> 0;
                    END TRY
                    BEGIN CATCH
                        THROW;
                    END CATCH;
                END'
            END";

        // 2. Trigger: TR_Employees_UpdateDepartmentEmployeeCount
        var triggerUpdateDepartmentEmployeeCount = @"
            IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = N'TR_Employees_UpdateDepartmentEmployeeCount')
            BEGIN
                EXEC dbo.sp_executesql @statement = N'
                CREATE TRIGGER [dbo].[TR_Employees_UpdateDepartmentEmployeeCount]
                ON [dbo].[Employees]
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    BEGIN TRY
                        ;WITH Changes AS
                        (
                            SELECT DepartmentId, COUNT(*) AS ChangeCount
                            FROM inserted
                            WHERE DepartmentId IS NOT NULL
                            GROUP BY DepartmentId

                            UNION ALL

                            SELECT DepartmentId, -COUNT(*) AS ChangeCount
                            FROM deleted
                            WHERE DepartmentId IS NOT NULL
                            GROUP BY DepartmentId
                        ),
                        DepartmentChanges AS
                        (
                            SELECT DepartmentId, SUM(ChangeCount) AS ChangeCount
                            FROM Changes
                            GROUP BY DepartmentId
                        )
                        UPDATE d
                        SET d.EmployeeCount = d.EmployeeCount + dc.ChangeCount
                        FROM dbo.Departments AS d
                        INNER JOIN DepartmentChanges AS dc
                            ON dc.DepartmentId = d.Id
                        WHERE dc.ChangeCount <> 0;

                    END TRY
                    BEGIN CATCH
                        THROW;
                    END CATCH;
                END'
            END";

        await context.Database.ExecuteSqlRawAsync(triggerUpdateDepartmentAssetCount);
        await context.Database.ExecuteSqlRawAsync(triggerUpdateDepartmentEmployeeCount);
    }
}