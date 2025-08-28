using HotChocolate.Types;
using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.GraphQL.Types;

/// <summary>
/// GraphQL type for Role entity
/// </summary>
public class RoleType : ObjectType<Role>
{
    protected override void Configure(IObjectTypeDescriptor<Role> descriptor)
    {
        descriptor.Name("Role");
        descriptor.Description("A role in the security audit system");

        descriptor.Field(r => r.Id)
            .Type<NonNullType<IdType>>()
            .Description("The unique identifier for the role");

        descriptor.Field(r => r.Name)
            .Type<NonNullType<StringType>>()
            .Description("The role name (e.g., BasicUser, AuthObserver, etc.)");

        descriptor.Field(r => r.Description)
            .Type<StringType>()
            .Description("Optional description of the role");

        descriptor.Field(r => r.Users)
            .Type<ListType<UserType>>()
            .Description("Users assigned to this role");

        descriptor.Field(r => r.RoleClaims)
            .Type<ListType<RoleClaimType>>()
            .Description("The role-claim relationships for this role");
    }
}
