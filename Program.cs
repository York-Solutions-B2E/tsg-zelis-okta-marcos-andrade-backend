using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SecurityAuditDashboard.Api.Authentication;
using SecurityAuditDashboard.Api.Authorization;
using SecurityAuditDashboard.Api.Data.Context;
using SecurityAuditDashboard.Api.Repositories.Interfaces;
using SecurityAuditDashboard.Api.Repositories.Implementations;
using SecurityAuditDashboard.Api.Services.Interfaces;
using SecurityAuditDashboard.Api.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Enable PII logging for debugging (REMOVE IN PRODUCTION!)
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with SQL Server
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Bind Authentication Configuration
var authConfig = new AuthenticationConfig();
builder.Configuration.GetSection("Authentication").Bind(authConfig);
builder.Services.AddSingleton(authConfig);

// Add JWT Bearer Authentication with multiple schemes for Okta and Microsoft
// Use a policy scheme to dynamically select between Okta and Microsoft
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "JWT";
        options.DefaultChallengeScheme = "JWT";
    })
    .AddPolicyScheme("JWT", "JWT", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            // Get the authorization header
            string authorization = context.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorization.Substring("Bearer ".Length).Trim();
                
                // Decode the JWT to check the issuer
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    var issuer = jsonToken?.Issuer;
                    
                    // Route to the appropriate handler based on issuer
                    if (issuer?.Contains("okta.com") == true)
                        return "Okta";
                    if (issuer?.Contains("sts.windows.net") == true || issuer?.Contains("microsoftonline.com") == true)
                        return "Microsoft";
                }
                catch { }
            }
            
            // Default to Okta if we can't determine
            return "Okta";
        };
    })
    .AddJwtBearer("Okta", options =>
    {
        options.Authority = "https://integrator-1262812.okta.com/oauth2/default";
        options.Audience = "api://default";
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,  
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        
        options.RequireHttpsMetadata = true;
        
        // Add logging for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Okta JWT Authentication failed: {Error}", context.Exception?.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Okta JWT Token validated successfully");
                
                // Log all claims to understand what's in the token
                if (context.Principal != null)
                {
                    foreach (var claim in context.Principal.Claims)
                    {
                        logger.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }
                    
                    var email = context.Principal.FindFirst("email")?.Value ?? 
                                context.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                    logger.LogInformation("User email from token: {Email}", email ?? "NOT FOUND");
                }
                
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (!string.IsNullOrEmpty(token))
                {
                    logger.LogDebug("JWT Bearer token received (first 20 chars): {Token}...", token?.Substring(0, Math.Min(20, token?.Length ?? 0)));
                }
                else
                {
                    logger.LogWarning("No Bearer token found in Authorization header");
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddJwtBearer("Microsoft", options =>
    {
        // Microsoft access tokens for our API
        // Use v1.0 authority since access tokens use v1.0 issuer format
        options.Authority = "https://sts.windows.net/ccff906d-efd7-4161-ae3a-72a8a92488ef/";
        options.Audience = "api://a4173a31-0e26-467a-abfe-0a564fdba2f3"; // Access token audience with api:// prefix
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        
        options.RequireHttpsMetadata = true;
        
        // Add logging for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Microsoft JWT Authentication failed: {Error}", context.Exception?.Message);
                
                // Let's see what token we're actually getting
                string authorization = context.Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authorization.Substring("Bearer ".Length).Trim();
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(token);
                        logger.LogError("Token Details - Issuer: {Issuer}, Audience: {Audience}, Subject: {Subject}",
                            jsonToken.Issuer, 
                            string.Join(", ", jsonToken.Audiences), 
                            jsonToken.Subject);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Could not read token: {Error}", ex.Message);
                    }
                }
                
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                
                // Check the audience to see if it's an access token or ID token
                var audience = context.Principal?.FindFirst("aud")?.Value ?? "unknown";
                var tokenType = audience.StartsWith("api://") ? "Access Token" : "ID Token";
                
                logger.LogInformation("Microsoft JWT validated - Type: {TokenType}, Audience: {Audience}", 
                    tokenType, audience);
                
                // Log all claims to understand what's in the token
                if (context.Principal != null)
                {
                    foreach (var claim in context.Principal.Claims)
                    {
                        logger.LogDebug("Microsoft Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }
                    
                    // Check for external ID claims
                    var uid = context.Principal.FindFirst("uid")?.Value;
                    var sub = context.Principal.FindFirst("sub")?.Value;
                    var oid = context.Principal.FindFirst("oid")?.Value;
                    logger.LogInformation("External IDs - uid: {Uid}, sub: {Sub}, oid: {Oid}", uid, sub, oid);
                }
                
                return Task.CompletedTask;
            }
        };
    });

// Add Authorization Handler
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Add Authorization with database-backed permission policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewAuthEvents", 
        policy => policy.Requirements.Add(new PermissionRequirement("Audit.ViewAuthEvents")));
    
    options.AddPolicy("CanViewRoleChanges", 
        policy => policy.Requirements.Add(new PermissionRequirement("Audit.RoleChanges")));
});

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISecurityEventRepository, SecurityEventRepository>();

// Add application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISecurityEventService, SecurityEventService>();

// Add Controllers
builder.Services.AddControllers();

// Add CORS for UI
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy.WithOrigins("http://localhost:5119")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add GraphQL Server
builder.Services
    .AddGraphQLServer()
    .AddQueryType<SecurityAuditDashboard.Api.GraphQL.Queries.Query>()
    .AddMutationType<SecurityAuditDashboard.Api.GraphQL.Mutations.Mutation>()
    .AddType<SecurityAuditDashboard.Api.GraphQL.Types.UserType>()
    .AddType<SecurityAuditDashboard.Api.GraphQL.Types.RoleType>()
    .AddType<SecurityAuditDashboard.Api.GraphQL.Types.ClaimType>()
    .AddType<SecurityAuditDashboard.Api.GraphQL.Types.RoleClaimType>()
    .AddType<SecurityAuditDashboard.Api.GraphQL.Types.SecurityEventType>()
    .AddType<SecurityAuditDashboard.Api.GraphQL.Types.RoleChangeType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization()
    .AddErrorFilter<SecurityAuditDashboard.Api.GraphQL.GraphQLErrorFilter>()
    .ModifyRequestOptions(opt => 
    {
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowUI");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map GraphQL endpoint
app.MapGraphQL("/graphql");

// Enable GraphQL IDE in development
if (app.Environment.IsDevelopment())
{
    app.MapBananaCakePop("/graphql-ide");
}

// Apply migrations and seed data on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
