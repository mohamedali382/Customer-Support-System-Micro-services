using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SupportService.Data;
using SupportService.Services;
using SupportService.Services.Interfaces;
using System.Security.Claims;
    

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<SupportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHostedService<RabbitMQConsumer>();

builder.Services.AddSingleton<RabbitMQPublisher>(sp =>
{
    var publisher = new RabbitMQPublisher(
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<ILogger<RabbitMQPublisher>>());
    publisher.InitializeAsync().GetAwaiter().GetResult();
    return publisher;
});


builder.Services.AddHttpClient<TicketServiceClient>();
builder.Services.AddHttpClient<IdentityServiceClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ITicketResponseService, TicketResponseService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        ))
            };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SupportDbContext>();
    for (var retry = 0; retry < 5; retry++)
    {
        try { db.Database.Migrate(); break; }
        catch (Microsoft.Data.SqlClient.SqlException) when (retry < 4)
        { Thread.Sleep(3000); }
    }
}

//app.UseHttpsRedirection();    
app.UseAuthentication();      
app.UseAuthorization();       


app.MapControllers();
app.Run();