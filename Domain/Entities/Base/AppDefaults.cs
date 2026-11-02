namespace Domain.Entities.Base;
public static class AppDefaults
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Employee = "Employee";
        public const string Client = "Client";
        public static readonly IReadOnlyList<string> AllRoles = new[] { Admin, Client, Manager, Employee };
    }
}