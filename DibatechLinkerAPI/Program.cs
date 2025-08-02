using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DibatechLinkerAPI.Data;
using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Services.Interfaces;
using DibatechLinkerAPI.Services.Implementations;
using AspNetCoreRateLimit;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.MemoryStorage;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Hangfire.PostgreSql; // Add this line

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (builder.Environment.IsDevelopment())
    {
        // UNCHANGED - Still uses SQLite for development
        var devConnectionString = builder.Configuration.GetConnectionString("SqliteConnection") 
            ?? "Data Source=dibatechlinker.db";
        options.UseSqlite(devConnectionString);
    }
    else
    {
        // ENHANCED - Checks connection string type for production
        if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("postgres"))
        {
            options.UseNpgsql(connectionString);  // For Render
        }
        else
        {
            options.UseSqlServer(connectionString);  // For Azure/AWS
        }
    }
});

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DibaTech Linker API",
        Version = "v1",
        Description = "A mobile-first web service for saving and organizing links with automatic content extraction and categorization.",
        Contact = new OpenApiContact
        {
            Name = "DibaTech.ng",
            Email = "contact@dibatech.ng",
            Url = new Uri("https://dibatech.ng")
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILinkParsingService, LinkParsingService>();
builder.Services.AddScoped<ISavedLinkService, SavedLinkService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

// Email Service - SendGrid integration for notifications
builder.Services.AddScoped<IEmailService, EmailService>();

// HttpClient for LinkParsingService
builder.Services.AddHttpClient<ILinkParsingService, LinkParsingService>();

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30); // For anonymous users
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Rate Limiting Configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(
            "https://linker.dibatech.ng",              // Correct subdomain for web version
            "https://dibatech-linker-web.onrender.com", // Render deployment URL
            "http://localhost:3000",                    // Local development
            "http://localhost:3001",                    // Alternative local port
            "http://localhost:5173"                     // Vite default port
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Hangfire Configuration - Simple version for quick deployment
builder.Services.AddHangfire(configuration => configuration
    .UseMemoryStorage()); // Use memory storage for all environments

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DibaTech Linker API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

// Add Rate Limiting Middleware (before authentication)
//app.UseIpRateLimiting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add Hangfire dashboard in development
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

// Seed data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Use migrations instead of EnsureCreated
    context.Database.Migrate();
}

// Configure port for Render deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
