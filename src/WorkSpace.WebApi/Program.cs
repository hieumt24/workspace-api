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
    options.AddPolicy("SignalRCorsPolicy", builder =>
    {
        builder
            .WithOrigins(
                "http://localhost:3000",  
                "http://localhost:5173"
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
    // Development: Allow all origins for easier testing
    app.UseCors("AllowAll");
}
else
{
    // Production: Only allow specific origins from appsettings
    app.UseCors("Production");
}
app.UseCors("SignalRCorsPolicy");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/orderHub");


app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<EnhancedChatHub>("/hubs/chat", options =>
{
    options.Transports = 
        Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
});
app.Run();