using Microsoft.EntityFrameworkCore;

namespace Persistence.Seeders;

public static class StoredProcedureSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        // 1. Stored Procedure: sp_GetAssetAgeDistribution
        var spAssetAgeDistribution = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetAssetAgeDistribution]') AND type in (N'P', N'PC'))
            BEGIN
                EXEC dbo.sp_executesql @statement = N'
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
                END'
            END";

        // 2. Stored Procedure: sp_GetDepartmentValueAnalytics
        var spDepartmentValueAnalytics = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetDepartmentValueAnalytics]') AND type in (N'P', N'PC'))
            BEGIN
                EXEC dbo.sp_executesql @statement = N'
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
                END'
            END";

        await context.Database.ExecuteSqlRawAsync(spAssetAgeDistribution);
        await context.Database.ExecuteSqlRawAsync(spDepartmentValueAnalytics);
    }
}