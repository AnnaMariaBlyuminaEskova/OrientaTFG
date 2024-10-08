using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrientaTFG.Shared.Infrastructure.DBContext;
using OrientaTFG.Shared.Infrastructure.Enums;
using System.Reflection;
using System.Security.Claims;

// Create a new web application builder with the provided command line arguments
var builder = WebApplication.CreateBuilder(args);

// Create a new configuration builder
var config = new ConfigurationBuilder();
// Add a JSON configuration file to the configuration builder
config.AddJsonFile("Autofac/configuration.json");

// Configure the host to use the Autofac service provider factory
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory((containerBuilder) =>
{
    // Register the OrientaTFGContext type with Autofac and configure it to be a single instance
    containerBuilder.RegisterType<OrientaTFGContext>().SingleInstance();
    // Register a new configuration module with Autofac using the built configuration
    containerBuilder.RegisterModule(new ConfigurationModule(config.Build()));
}));

// Add controllers and API explorer endpoints to the service collection
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add JWT authentication to the service collection
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Use the secret key from the configuration to validate the JWT token
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["SecretKey"])),
            RoleClaimType = ClaimTypes.Role
        };
    });

// Configure roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(nameof(RoleEnum.Tutor), policy =>
    {
        policy.RequireRole(nameof(RoleEnum.Tutor));
    });

    options.AddPolicy(nameof(RoleEnum.Estudiante), policy =>
    {
        policy.RequireRole(nameof(RoleEnum.Estudiante));
    });

    options.AddPolicy(nameof(RoleEnum.Administrator), policy =>
    {
        policy.RequireRole(nameof(RoleEnum.Administrator));
    });
});

// Configure Swagger for the service collection
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Api", Version = "v1" });

    // Add JWT security definition to Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Input your JWT token in textbox below",
        Name = "Authorization",
        In = ParameterLocation.Header
    };
    c.AddSecurityDefinition("Bearer", securityScheme);

    // Add JWT security requirement to Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsightsKey"]);

// Configure CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAllOrigins",
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Build the web application
var app = builder.Build();

// If the environment is development
if (app.Environment.IsDevelopment())
{
    // Use Swagger and configure the Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");

    });
}

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Use CORS
app.UseCors("AllowAllOrigins");

// Map the controllers
app.MapControllers();

// Run the application
app.Run();



