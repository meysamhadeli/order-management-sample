namespace OrderManagement.Identities.Constants;

public static class IdentityConstant
{
    public static class Role
    {
        public const string Admin = "admin";
        public const string User = "user";
    }

    public static class StandardScopes
    {
        public const string OrderManagementApi = "order-management-api";
    }
}
