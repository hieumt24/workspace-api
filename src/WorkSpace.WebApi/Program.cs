using Serilog;
using WorkSpace.Application.Extensions;
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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");
app.Run();