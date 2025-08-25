using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SecurityAuditDashboard.Api.Authentication;
using SecurityAuditDashboard.Api.Data.Context;
using SecurityAuditDashboard.Api.Repositories;
using SecurityAuditDashboard.Api.Services;

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

// Add Authentication Services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Okta";
})
.AddCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect("Okta", options =>
{
    options.Authority = authConfig.Okta.Authority;
    options.ClientId = authConfig.Okta.ClientId;
    options.ClientSecret = authConfig.Okta.ClientSecret;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.CallbackPath = "/signin-okta";
    options.SignedOutCallbackPath = "/signout-callback-okta";
    options.TokenValidationParameters.NameClaimType = "name";
    
    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async context =>
        {
            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            await userService.HandleLoginAsync(context.Principal!, "Okta");
        }
    };
})
.AddOpenIdConnect("Google", options =>
{
    options.Authority = "https://accounts.google.com";
    options.ClientId = authConfig.Google.ClientId;
    options.ClientSecret = authConfig.Google.ClientSecret;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.CallbackPath = "/signin-google";
    options.SignedOutCallbackPath = "/signout-callback-google";
    
    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async context =>
        {
            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            await userService.HandleLoginAsync(context.Principal!, "Google");
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewAuthEvents", policy =>
        policy.RequireClaim("permissions", "Audit.ViewAuthEvents"));
    
    options.AddPolicy("CanViewRoleChanges", policy =>
        policy.RequireClaim("permissions", "Audit.RoleChanges"));
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations and seed data on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
