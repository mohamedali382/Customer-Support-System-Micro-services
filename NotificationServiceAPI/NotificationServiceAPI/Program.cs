using Microsoft.EntityFrameworkCore;
using NotificationServiceAPI.Data;
using NotificationServiceAPI.Services;
using NotificationServiceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// =========================
// SERVICES
// =========================
builder.Services.AddScoped<InotificationService, NotificationService>();

// RabbitMQ background consumer
builder.Services.AddHostedService<RabbitMQConsumer>();

builder.Services.AddControllers();

// =========================
// CORS (for frontend later)
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =========================
// JWT AUTHENTICATION
// =========================
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey!)
        )
    };
});

builder.Services.AddAuthorization();


var app = builder.Build();


app.UseCors("AllowAll");

// IMPORTANT ORDER:
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    for (var retry = 0; retry < 5; retry++)
    {
        try { db.Database.Migrate(); break; }
        catch (Microsoft.Data.SqlClient.SqlException) when (retry < 4)
        { Thread.Sleep(3000); }
    }
}

app.Run();