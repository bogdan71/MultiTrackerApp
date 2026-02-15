using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracker.Api.Data;

namespace Tracker.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;

    public CustomWebApplicationFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"TrackerTest_{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TrackerDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Remove any existing DbContext registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TrackerDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            services.AddDbContext<TrackerDbContext>(options =>
            {
                options.UseSqlite($"DataSource={_dbPath}");
            });

            // Build the service provider and ensure the DB is created with seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TrackerDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Creates an HttpClient that is authenticated as a test user.
    /// Registers and logs in automatically, returning a client with Bearer token set.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(
        string email = "test@example.com",
        string password = "TestPass123!")
    {
        var client = CreateClient();

        // Register the user (ignore if already exists)
        await client.PostAsJsonAsync("/api/register", new { email, password });

        // Login to get the token
        var loginResponse = await client.PostAsJsonAsync("/api/login", new { email, password });
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(TestJsonOptions.Default);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            try { File.Delete(_dbPath); } catch { /* best effort cleanup */ }
        }
    }

    private record LoginResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);
}
