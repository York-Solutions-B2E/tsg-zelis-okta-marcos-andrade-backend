using HotChocolate.Types;
using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.GraphQL.Types;

/// <summary>
/// Represents a role change in a security event
/// </summary>
public class RoleChange
{
    public Guid PreviousRoleId { get; set; }
    public Role? PreviousRole { get; set; }
    
    public Guid NewRoleId { get; set; }
    public Role? NewRole { get; set; }
}

/// <summary>
/// GraphQL type for role change information
/// </summary>
public class RoleChangeType : ObjectType<RoleChange>
{
    protected override void Configure(IObjectTypeDescriptor<RoleChange> descriptor)
    {
        descriptor.Name("RoleChange");
        descriptor.Description("Information about a role change in a security event");

        descriptor.Field(rc => rc.PreviousRoleId)
            .Type<NonNullType<IdType>>()
            .Description("The ID of the previous role");

        descriptor.Field(rc => rc.PreviousRole)
            .Type<RoleType>()
            .Description("The previous role");

        descriptor.Field(rc => rc.NewRoleId)
            .Type<NonNullType<IdType>>()
            .Description("The ID of the new role");

        descriptor.Field(rc => rc.NewRole)
            .Type<RoleType>()
            .Description("The new role");
    }
}
