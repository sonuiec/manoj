using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FactFinderWeb.API.Services;
using FactFinderWeb.API.Models;
using FactFinderWeb.API.Utils;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ✅ Get allowed origins from appsettings.json if present
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

    // ✅ Add Controllers
    builder.Services.AddControllersWithViews();

    // ✅ Register DbContext
    builder.Services.AddDbContext<FactFinderDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("FactFinderDbCon")));

    // ✅ Register custom services
    builder.Services.AddScoped<JSONDataUtility>();
    builder.Services.AddSingleton<JwtService>();

    // ✅ Swagger configuration
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "FactFinder API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization using Bearer scheme. Example: 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

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
                new string[] {}
            }
        });
    });

    // ✅ JWT Authentication
    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true
            };
        });

    builder.Services.AddAuthorization();

    // ✅ CORS Configuration
    const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(MyAllowSpecificOrigins, policy =>
        {
            policy
                .WithOrigins(
                    "https://ffapi.mainstream.co.in",
                    "https://awaken.mainstream.co.in",
                    "https://localhost:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // ✅ Build the app
    var app = builder.Build();

    // ✅ Use CORS
    app.UseCors(MyAllowSpecificOrigins);

    // ✅ Swagger (enable always or only in Development)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FactFinder API v1"));

    // ✅ Use HTTPS redirection
    app.UseHttpsRedirection();

    // ✅ Authentication & Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // ✅ Map Controllers
    app.MapControllers();

    // ✅ Fallback (optional for SPA)
    app.MapFallbackToFile("/index.html");

    app.Run();
}
catch (Exception ex)
{
    File.WriteAllText("startup-error.txt", ex.ToString());
    throw;
}
