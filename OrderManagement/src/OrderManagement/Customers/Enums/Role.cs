using System.Text.Json.Serialization;

namespace OrderManagement.Customers.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Admin,
    User
}
