/*using Microsoft.EntityFrameworkCore.Migrations;

namespace Persistence.Data.Build.DataBase;

public class AssetAgeDistributionMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE PROCEDURE [dbo].[sp_GetAssetAgeDistribution]
                @DepartmentId UNIQUEIDENTIFIER = NULL
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT 
                    SUM(CASE WHEN DATEDIFF(year, a.PurchaseDate, GETDATE()) < 1 THEN 1 ELSE 0 END) AS UnderOne,
                    SUM(CASE WHEN DATEDIFF(year, a.PurchaseDate, GETDATE()) >= 1 AND DATEDIFF(year, a.PurchaseDate, GETDATE()) < 3 THEN 1 ELSE 0 END) AS OneToThree,
                    SUM(CASE WHEN DATEDIFF(year, a.PurchaseDate, GETDATE()) >= 3 AND DATEDIFF(year, a.PurchaseDate, GETDATE()) < 5 THEN 1 ELSE 0 END) AS ThreeToFive,
                    SUM(CASE WHEN DATEDIFF(year, a.PurchaseDate, GETDATE()) >= 5 THEN 1 ELSE 0 END) AS OverFive
                FROM Assets a
                WHERE (@DepartmentId IS NULL OR a.DepartmentId = @DepartmentId);
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP PROCEDURE [dbo].[sp_GetAssetAgeDistribution];
            """);
    }
}
*/