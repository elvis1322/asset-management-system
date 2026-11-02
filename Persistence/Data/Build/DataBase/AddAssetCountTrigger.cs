/*using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddAssetCountTrigger : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TRIGGER [dbo].[TR_Assets_UpdateDepartmentAssetCount]
            ON [dbo].[Assets]
            AFTER INSERT, UPDATE, DELETE
            AS
            BEGIN
                SET NOCOUNT ON;

                BEGIN TRY

                    ;WITH Changes AS
                    (
                        -- Assets të shtuara ose DepartmentId i ri
                        SELECT
                            DepartmentId,
                            COUNT(*) AS ChangeCount
                        FROM inserted
                        WHERE DepartmentId IS NOT NULL
                        GROUP BY DepartmentId

                        UNION ALL

                        -- Assets të fshira ose DepartmentId i vjetër
                        SELECT
                            DepartmentId,
                            -COUNT(*) AS ChangeCount
                        FROM deleted
                        WHERE DepartmentId IS NOT NULL
                        GROUP BY DepartmentId
                    ),
                    DepartmentChanges AS
                    (
                        -- Bashko ndryshimet për secilin departament
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
            END;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE [dbo].[Assets]
            ENABLE TRIGGER [TR_Assets_UpdateDepartmentAssetCount];
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TRIGGER IF EXISTS
            [dbo].[TR_Assets_UpdateDepartmentAssetCount];
            """);
    }
}
*/