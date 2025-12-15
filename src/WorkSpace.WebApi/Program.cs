using Serilog;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Hubs;
using WorkSpace.Infrastructure;
using WorkSpace.Infrastructure.Seeds;
using WorkSpace.WebApi.Extensions;
using WorkSpace.WebApi.Hubs;
using WorkSpace.WebApi.Middlewares;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.AddPresentation();
// Add SignalR with configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    
    options.MaximumReceiveMessageSize = 1024 * 1024;
});

builder.Services.AddCors(options =>
{
    // Development policy - allow local testing
    options.AddPolicy("Development", policy =>
    {
        policy
            .WithOrigins(
                // VS Code Live Server
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                // Python http.server
                "http://localhost:8080",
                "http://127.0.0.1:8080",
                // React/Vite dev servers
                "http://localhost:3000",
                "http://localhost:5173",
                // Backend URLs
                "https://localhost:44361",
                "https://localhost:7105",
                "http://localhost:7949",
                "http://localhost:5022"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    // Production policy - specify your production domains
    options.AddPolicy("Production", policy =>
    {
        policy
            .WithOrigins(
                "https://your-production-domain.com",
                "https://www.your-production-domain.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


builder.Logging.ClearProviders();
builder.Host.UseSerilog();
var app = builder.Build();


var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IWorkSpaceSeeder>();
await seeder.SeedAsync();

// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS - Phải đặt TRƯỚC UseAuthentication
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/orderHub");

// FIXED: Only one hub per path
app.MapHub<ChatHub>("/hubs/chat");

// If you need EnhancedChatHub, use different path:
app.MapHub<EnhancedChatHub>("/hubs/enhanced-chat", options =>
{
    options.Transports = 
        Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
});

// Customer Chat Hub
app.MapHub<CustomerChatHub>("/hubs/customer-chat", options =>
{
    options.Transports = 
        Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
});


app.Run();