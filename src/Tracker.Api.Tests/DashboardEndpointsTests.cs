using System.Net;
using System.Text.Json;

namespace Tracker.Api.Tests;

[TestClass]
public class DashboardEndpointsTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task GetDashboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/dashboard");
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetDashboard_ContainsSummarySection()
    {
        var response = await _client.GetAsync("/api/dashboard");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.IsTrue(root.TryGetProperty("summary", out var summary), "Dashboard should have a 'summary' property");
        Assert.IsTrue(summary.TryGetProperty("books", out _), "Summary should have 'books'");
        Assert.IsTrue(summary.TryGetProperty("movies", out _), "Summary should have 'movies'");
        Assert.IsTrue(summary.TryGetProperty("songs", out _), "Summary should have 'songs'");
        Assert.IsTrue(summary.TryGetProperty("todos", out var todos), "Summary should have 'todos'");
        Assert.IsTrue(summary.TryGetProperty("categories", out _), "Summary should have 'categories'");

        // Validate todos sub-structure
        Assert.IsTrue(todos.TryGetProperty("total", out _), "Todos should have 'total'");
        Assert.IsTrue(todos.TryGetProperty("completed", out _), "Todos should have 'completed'");
        Assert.IsTrue(todos.TryGetProperty("pending", out _), "Todos should have 'pending'");
        Assert.IsTrue(todos.TryGetProperty("highPriority", out _), "Todos should have 'highPriority'");
    }

    [TestMethod]
    public async Task GetDashboard_ContainsUpcomingSection()
    {
        var response = await _client.GetAsync("/api/dashboard");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.IsTrue(root.TryGetProperty("upcoming", out var upcoming), "Dashboard should have an 'upcoming' property");
        Assert.IsTrue(upcoming.TryGetProperty("books", out _), "Upcoming should have 'books'");
        Assert.IsTrue(upcoming.TryGetProperty("movies", out _), "Upcoming should have 'movies'");
        Assert.IsTrue(upcoming.TryGetProperty("songs", out _), "Upcoming should have 'songs'");
        Assert.IsTrue(upcoming.TryGetProperty("recentItems", out _), "Upcoming should have 'recentItems'");
    }

    [TestMethod]
    public async Task GetDashboard_ContainsPendingTodosSection()
    {
        var response = await _client.GetAsync("/api/dashboard");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.IsTrue(root.TryGetProperty("pendingTodos", out var pendingTodos), "Dashboard should have 'pendingTodos'");
        Assert.AreEqual(JsonValueKind.Array, pendingTodos.ValueKind, "PendingTodos should be an array");
    }

    [TestMethod]
    public async Task GetDashboard_TodoCountsAreAccurate()
    {
        var response = await _client.GetAsync("/api/dashboard");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var todos = doc.RootElement.GetProperty("summary").GetProperty("todos");
        var total = todos.GetProperty("total").GetInt32();
        var completed = todos.GetProperty("completed").GetInt32();
        var pending = todos.GetProperty("pending").GetInt32();

        Assert.IsTrue(total >= 3, "Should have at least 3 seeded todos");
        Assert.AreEqual(total, completed + pending, "Total should equal completed + pending");
    }
}
