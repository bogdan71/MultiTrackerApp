using Microsoft.EntityFrameworkCore;
using Tracker.Api.Data;
using Tracker.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddDbContext<TrackerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TrackerDb")
        ?? "Data Source=tracker.db"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migrate and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TrackerDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors();
app.MapDefaultEndpoints();

app.MapBookEndpoints();
app.MapMovieEndpoints();
app.MapSongEndpoints();
app.MapTodoEndpoints();
app.MapDashboardEndpoints();
app.MapCategoryEndpoints();
app.MapItemEndpoints();

app.Run();
