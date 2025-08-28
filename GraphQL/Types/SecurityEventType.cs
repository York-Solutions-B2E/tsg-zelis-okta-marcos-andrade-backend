using HotChocolate.Types;
using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.GraphQL.Types;

/// <summary>
/// GraphQL type for SecurityEvent entity
/// </summary>
public class SecurityEventType : ObjectType<SecurityEvent>
{
    protected override void Configure(IObjectTypeDescriptor<SecurityEvent> descriptor)
    {
        descriptor.Name("SecurityEvent");
        descriptor.Description("A security-related event in the audit system");

        descriptor.Field(se => se.Id)
            .Type<NonNullType<IdType>>()
            .Description("The unique identifier for the security event");

        descriptor.Field(se => se.EventType)
            .Type<NonNullType<StringType>>()
            .Description("The type of security event (LOGIN_SUCCESS, ROLE_ASSIGNED, etc.)");

        descriptor.Field(se => se.AuthorUserId)
            .Type<NonNullType<IdType>>()
            .Description("The ID of the user who performed the action");

        descriptor.Field(se => se.AuthorUser)
            .Type<UserType>()
            .Description("The user who performed the action");

        descriptor.Field(se => se.AffectedUserId)
            .Type<NonNullType<IdType>>()
            .Description("The ID of the user who was affected by the action");

        descriptor.Field(se => se.AffectedUser)
            .Type<UserType>()
            .Description("The user who was affected by the action");

        descriptor.Field(se => se.OccurredUtc)
            .Type<NonNullType<DateTimeType>>()
            .Description("When the security event occurred (UTC)");

        descriptor.Field(se => se.Details)
            .Type<StringType>()
            .Description("Additional details about the security event");

        // Role change specific fields
        descriptor.Field(se => se.PreviousRoleId)
            .Type<IdType>()
            .Description("The ID of the previous role (only for ROLE_ASSIGNED events)");

        descriptor.Field(se => se.PreviousRole)
            .Type<RoleType>()
            .Description("The previous role (only for ROLE_ASSIGNED events)");

        descriptor.Field(se => se.NewRoleId)
            .Type<IdType>()
            .Description("The ID of the new role (only for ROLE_ASSIGNED events)");

        descriptor.Field(se => se.NewRole)
            .Type<RoleType>()
            .Description("The new role (only for ROLE_ASSIGNED events)");

        // Computed field for role change information
        descriptor.Field("roleChange")
            .Type<RoleChangeType>()
            .Description("Role change information (only available for ROLE_ASSIGNED events)")
            .Resolve(context =>
            {
                var securityEvent = context.Parent<SecurityEvent>();
                
                if (securityEvent.EventType == "ROLE_ASSIGNED" && 
                    securityEvent.PreviousRoleId.HasValue && 
                    securityEvent.NewRoleId.HasValue)
                {
                    return new RoleChange
                    {
                        PreviousRoleId = securityEvent.PreviousRoleId.Value,
                        NewRoleId = securityEvent.NewRoleId.Value,
                        PreviousRole = securityEvent.PreviousRole,
                        NewRole = securityEvent.NewRole
                    };
                }
                
                return null;
            });
    }
}
