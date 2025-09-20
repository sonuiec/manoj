
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FactFinderWeb.API.Services;

var builder = WebApplication.CreateBuilder(args);
//call connection string
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

 
//var connString = builder.Configuration.GetConnectionString("AppDb");

//var r = "DB Connection: " + builder.Configuration.GetConnectionString("AppDb");
// Add services to the container.{our project}


builder.Services.AddSingleton<JwtService>();
//builder.Services.AddControllers().AddNewtonsoftJson(); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//call cors start
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod().AllowCredentials()); // Required for cookies/auth headers
});
builder.Services.AddControllersWithViews();

builder.Services.AddSwaggerGen(
               c =>
               {
                   c.SwaggerDoc("v1", new OpenApiInfo { Title = "BaseWebApi", Version = "v1" });
                   c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                   {
                       Description = "Jwt Authorization",
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
                            Id= "Bearer"
                        }
                    },
                    new string[]{}
                    }
               });
               });


// JWT Authentication Configuration
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

//end JWT

builder.Services.AddAuthorization();

 
var app = builder.Build();
 
app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoJWTToken v1"));
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.MapFallbackToFile("/index.html");

app.Run();
