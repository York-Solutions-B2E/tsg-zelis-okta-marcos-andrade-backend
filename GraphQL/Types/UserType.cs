using HotChocolate.Types;
using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.GraphQL.Types;

/// <summary>
/// GraphQL type for User entity
/// </summary>
public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Name("User");
        descriptor.Description("A user in the security audit system");

        descriptor.Field(u => u.Id)
            .Type<NonNullType<IdType>>()
            .Description("The unique identifier for the user");

        descriptor.Field(u => u.Email)
            .Type<NonNullType<StringType>>()
            .Description("The user's email address");

        descriptor.Field(u => u.Name)
            .Type<NonNullType<StringType>>()
            .Description("The user's display name");

        descriptor.Field(u => u.ExternalId)
            .Type<NonNullType<StringType>>()
            .Description("The external authentication provider ID");

        descriptor.Field(u => u.Role)
            .Description("The user's assigned role");

        descriptor.Field(u => u.CreatedAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("When the user was created");

        descriptor.Field(u => u.LastLoginAt)
            .Type<DateTimeType>()
            .Description("When the user last logged in");
    }
}
