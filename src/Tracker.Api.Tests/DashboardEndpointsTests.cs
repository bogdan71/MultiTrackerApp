using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tracker.Api.Tests;

[TestClass]
public class DashboardEndpointsTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory();
        _client = await _factory.CreateAuthenticatedClientAsync("dashtest@example.com", "TestPass123!");
    }

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    public async Task Unauthenticated_Returns401()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/dashboard");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetDashboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/dashboard");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetDashboard_ContainsSummarySection()
    {
        var dashboard = await _client.GetFromJsonAsync<JsonElement>("/api/dashboard", TestJsonOptions.Default);
        Assert.IsTrue(dashboard.TryGetProperty("summary", out _));
    }

    [TestMethod]
    public async Task GetDashboard_ContainsUpcomingSection()
    {
        var dashboard = await _client.GetFromJsonAsync<JsonElement>("/api/dashboard", TestJsonOptions.Default);
        Assert.IsTrue(dashboard.TryGetProperty("upcoming", out _));
    }

    [TestMethod]
    public async Task GetDashboard_ContainsPendingTodosSection()
    {
        var dashboard = await _client.GetFromJsonAsync<JsonElement>("/api/dashboard", TestJsonOptions.Default);
        Assert.IsTrue(dashboard.TryGetProperty("pendingTodos", out _));
    }

    [TestMethod]
    public async Task GetDashboard_ReflectsCreatedData()
    {
        // Create some data
        await _client.PostAsJsonAsync("/api/books", new { Title = "Dash Book", Author = "A", Genre = "X", Status = "Upcoming" });
        await _client.PostAsJsonAsync("/api/todos", new { Title = "Dash Todo", Priority = "High" });

        var dashboard = await _client.GetFromJsonAsync<JsonElement>("/api/dashboard", TestJsonOptions.Default);
        var summary = dashboard.GetProperty("summary");
        Assert.IsTrue(summary.TryGetProperty("books", out _));
        Assert.IsTrue(summary.TryGetProperty("todos", out _));
    }
}
