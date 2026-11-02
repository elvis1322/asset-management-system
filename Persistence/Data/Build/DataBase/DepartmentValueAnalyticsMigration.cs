/*using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Data.Build.DataBase;

public class DepartmentValueAnalyticsMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE PROCEDURE [dbo].[sp_GetDepartmentValueAnalytics]
                @DepartmentId UNIQUEIDENTIFIER = NULL
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    d.Name AS DepartmentName,

                    CAST(ISNULL(a.TotalValue, 0) AS DECIMAL(18, 2)) AS TotalValue,

                    d.AssetCount AS AssetCount,

                    d.EmployeeCount AS EmployeeCount

                FROM dbo.Departments d

                OUTER APPLY
                (
                    SELECT SUM(ast.PurchaseValue) AS TotalValue
                    FROM dbo.Assets ast
                    WHERE ast.DepartmentId = d.Id
                ) a

                WHERE
                    @DepartmentId IS NULL
                    OR d.Id = @DepartmentId;
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP PROCEDURE [dbo].[sp_GetDepartmentValueAnalytics];
            """);
    }
}
*/