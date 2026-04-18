using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Playspot.Application.Interfaces;
using Playspot.Infrastructure.Data;
using Playspot.Infrastructure.Services;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IJoinRequestService, JoinRequestService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(); // UI at /scalar/v1
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();