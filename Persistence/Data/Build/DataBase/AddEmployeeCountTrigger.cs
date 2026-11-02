/*using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddEmployeeCountTrigger : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
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
            END;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE [dbo].[Employees]
            ENABLE TRIGGER [TR_Employees_UpdateDepartmentEmployeeCount];
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TRIGGER IF EXISTS
            [dbo].[TR_Employees_UpdateDepartmentEmployeeCount];
            """);
    }
}
*/