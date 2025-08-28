using HotChocolate.Types;
using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.GraphQL.Types;

/// <summary>
/// GraphQL type for RoleClaim junction entity
/// </summary>
public class RoleClaimType : ObjectType<RoleClaim>
{
    protected override void Configure(IObjectTypeDescriptor<RoleClaim> descriptor)
    {
        descriptor.Name("RoleClaim");
        descriptor.Description("Junction entity linking roles and claims");

        descriptor.Field(rc => rc.RoleId)
            .Type<NonNullType<IdType>>()
            .Description("The role ID");

        descriptor.Field(rc => rc.Role)
            .Type<RoleType>()
            .Description("The role");

        descriptor.Field(rc => rc.ClaimId)
            .Type<NonNullType<IdType>>()
            .Description("The claim ID");

        descriptor.Field(rc => rc.Claim)
            .Type<ClaimType>()
            .Description("The claim");
    }
}
