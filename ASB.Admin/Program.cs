using Microsoft.OpenApi;
using Serilog;
using ASB.Services.v1.Interfaces;
using ASB.Services.v1.Implementations;
using Microsoft.EntityFrameworkCore;
using ASB.Repositories.v1.Contexts;
using ASB.Repositories.v1.Interfaces;
using ASB.Repositories.v1.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ASB.Admin.v1.Infrastructure;
using ASB.Authorization;



const string schemeId = "Bearer";
var builder = WebApplication.CreateBuilder(args);

// Define a consistent log output template
var logOutputTemplate = "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message:lj}{NewLine}{Exception}";
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // allows config from appsettings.json
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: logOutputTemplate)
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: logOutputTemplate)
    .CreateLogger();

builder.Host.UseSerilog(); // replace default logger

builder.Services.AddDbContext<AsbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AsbDatabase"),
        b => b.MigrationsAssembly("ASB.Admin")
           .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null))
        );


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IUserGroupRepository, UserGroupRepository>();
builder.Services.AddScoped<IAuthTokenService, AuthTokenService>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IKeycloakUserProvisioningService, KeycloakUserProvisioningService>();
builder.Services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>();

// Register policy-based authorization
builder.Services.AddPolicyAuthorization();

//Swagger configuration
// Configure Swagger with authentication
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference(schemeId, document),
            []
        }
    });
});
// ── JWT Bearer (validates app tokens signed with Jwt:Secret) ─────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "asb-api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "asb-client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateAudience         = true,
            ValidAudience            = jwtAudience,
            ValidateIssuer           = true,
            ValidIssuer              = jwtIssuer,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
            NameClaimType            = "preferred_username",
            RoleClaimType            = "roles"
        };
    });

var app = builder.Build();

app.UseCors("AllowAll");

app.UseExceptionHandler("/error");

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();