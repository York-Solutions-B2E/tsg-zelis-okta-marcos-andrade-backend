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

// Add JWT Bearer Authentication following Microsoft BFF pattern
// Configure to accept tokens from Okta (primary) and Microsoft (secondary)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure JWT validation for Okta tokens
        // The Authority tells the middleware where to download the OIDC metadata (including signing keys)
        options.Authority = "https://integrator-1262812.okta.com/oauth2/default";
        
        // The audience should match what's configured in Okta
        options.Audience = "api://default"; // This is typically the default for Okta
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,  
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        
        options.RequireHttpsMetadata = true; // Okta uses HTTPS
        
        // Add logging for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("JWT Authentication failed: {Error}", context.Exception?.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("JWT Token validated successfully");
                
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
    .AddErrorFilter<SecurityAuditDashboard.Api.GraphQL.GraphQLErrorFilter>();

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
