using HotChocolate.Types;
using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.GraphQL.Types;

/// <summary>
/// GraphQL type for Claim entity
/// </summary>
public class ClaimType : ObjectType<Claim>
{
    protected override void Configure(IObjectTypeDescriptor<Claim> descriptor)
    {
        descriptor.Name("Claim");
        descriptor.Description("A permission claim in the security audit system");

        descriptor.Field(c => c.Id)
            .Type<NonNullType<IdType>>()
            .Description("The unique identifier for the claim");

        descriptor.Field(c => c.Type)
            .Type<NonNullType<StringType>>()
            .Description("The claim type (e.g., 'permission')");

        descriptor.Field(c => c.Value)
            .Type<NonNullType<StringType>>()
            .Description("The claim value (e.g., 'CanViewAuthEvents')");

        descriptor.Field(c => c.RoleClaims)
            .Type<ListType<RoleClaimType>>()
            .Description("The role-claim relationships for this claim");
    }
}
