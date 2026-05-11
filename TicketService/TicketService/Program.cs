using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.Services;
using TicketService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        ))
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<RabbitMQPublisher>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var publisher = new RabbitMQPublisher(config);
    publisher.InitializeAsync().GetAwaiter().GetResult(); ;
    return publisher;
});

builder.Services.AddScoped<IticketService, TicketService.Services.TicketService>();

var app = builder.Build();

app.UseCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    for (var retry = 0; retry < 5; retry++)
    {
        try { db.Database.Migrate(); break; }
        catch (Microsoft.Data.SqlClient.SqlException) when (retry < 4)
        { Thread.Sleep(3000); }
    }
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
