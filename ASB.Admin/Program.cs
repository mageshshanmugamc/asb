using Microsoft.OpenApi;
using Serilog;
using ASB.Services.v1;
using ASB.Repositories.v1;
using ASB.Agent.v1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ASB.Authorization;
using ASB.ErrorHandler.v1.Extensions;
using FluentValidation;
using ASB.Admin.v1.Filters;
using Microsoft.EntityFrameworkCore;



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

// Register database provider (SqlServer or Neo4j) based on config
builder.Services.AddDatabase(builder.Configuration);

// Register FluentValidation validators from this assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});


builder.Services.AddApplicationServices();

// Register AI Agent services (Semantic Kernel + Ollama + Qdrant)
builder.Services.AddAgentServices(builder.Configuration);

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

// Auto-apply pending EF Core migrations (creates the DB if it doesn't exist)
if (app.Configuration["DatabaseProvider"]?.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ASB.Repositories.v1.Contexts.AsbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowAll");

app.UseAppErrorHandler();

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