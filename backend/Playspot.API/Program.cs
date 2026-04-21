using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Playspot.Application.Interfaces;
using Playspot.Infrastructure.Data;
using Playspot.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register IAppDbContext → AppDbContext
builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// ── MediatR ──
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IAppDbContext).Assembly));

// ── Infrastructure Services ──
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(
                "http://localhost:5500",     // VS Code Live Server
                "http://127.0.0.1:5500",    // VS Code Live Server (alt)
                "http://localhost:3000",     // Common dev server
                "http://localhost:8080"      // Other dev server
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());

    // Fallback for any origin (development convenience)
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── JWT Authentication ──
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Use AllowAll for development, use default policy for production
if (app.Environment.IsDevelopment())
    app.UseCors("AllowAll");
else
    app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();