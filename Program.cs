using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; 
using BattleShip.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to include console logging
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders(); // Clears default logging providers
    logging.AddConsole(); // Adds console logging
});

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// HTTP pipeline configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<GameHub>("/gameHub");

app.Run();