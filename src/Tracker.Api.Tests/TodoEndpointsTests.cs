using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class TodoEndpointsTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory();
        _client = await _factory.CreateAuthenticatedClientAsync("todotest@example.com", "TestPass123!");
    }

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    public async Task Unauthenticated_Returns401()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/todos");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_ReturnsEmptyList()
    {
        var todos = await _client.GetFromJsonAsync<TodoItem[]>("/api/todos", TestJsonOptions.Default);
        Assert.IsNotNull(todos);
    }

    [TestMethod]
    public async Task CreateAndGet_ReturnsTodo()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { Title = "Test Todo", Priority = "High", Category = "Work" });
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Todo", created.Title);
        Assert.AreEqual(Priority.High, created.Priority);

        var fetched = await _client.GetFromJsonAsync<TodoItem>($"/api/todos/{created.Id}", TestJsonOptions.Default);
        Assert.AreEqual("Test Todo", fetched!.Title);
    }

    [TestMethod]
    public async Task Update_ReturnsUpdatedTodo()
    {
        var createResp = await _client.PostAsJsonAsync("/api/todos", new { Title = "Update Me", Priority = "Low" });
        var created = await createResp.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);

        var updateResp = await _client.PutAsJsonAsync($"/api/todos/{created!.Id}", new { Title = "Updated", Priority = "Critical", IsCompleted = true });
        Assert.AreEqual(HttpStatusCode.OK, updateResp.StatusCode);

        var updated = await updateResp.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.AreEqual("Updated", updated!.Title);
        Assert.AreEqual(Priority.Critical, updated.Priority);
        Assert.IsTrue(updated.IsCompleted);
    }

    [TestMethod]
    public async Task Toggle_FlipsCompletion()
    {
        var createResp = await _client.PostAsJsonAsync("/api/todos", new { Title = "Toggle Me", Priority = "Medium" });
        var created = await createResp.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsFalse(created!.IsCompleted);

        var toggleResp = await _client.PatchAsync($"/api/todos/{created.Id}/toggle", null);
        var toggled = await toggleResp.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsTrue(toggled!.IsCompleted);

        var toggleResp2 = await _client.PatchAsync($"/api/todos/{created.Id}/toggle", null);
        var toggled2 = await toggleResp2.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsFalse(toggled2!.IsCompleted);
    }

    [TestMethod]
    public async Task Delete_ReturnsNoContent()
    {
        var createResp = await _client.PostAsJsonAsync("/api/todos", new { Title = "Delete Me", Priority = "Low" });
        var created = await createResp.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);

        var deleteResp = await _client.DeleteAsync($"/api/todos/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [TestMethod]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/todos/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task FilterByPriority_ReturnsFilteredTodos()
    {
        await _client.PostAsJsonAsync("/api/todos", new { Title = "FP", Priority = "Critical" });
        var todos = await _client.GetFromJsonAsync<TodoItem[]>("/api/todos?priority=Critical", TestJsonOptions.Default);
        Assert.IsTrue(todos!.All(t => t.Priority == Priority.Critical));
    }

    [TestMethod]
    public async Task FilterByCompleted_ReturnsFilteredTodos()
    {
        var createResp = await _client.PostAsJsonAsync("/api/todos", new { Title = "FC", Priority = "Low" });
        var created = await createResp.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        await _client.PatchAsync($"/api/todos/{created!.Id}/toggle", null);

        var todos = await _client.GetFromJsonAsync<TodoItem[]>("/api/todos?completed=true", TestJsonOptions.Default);
        Assert.IsTrue(todos!.All(t => t.IsCompleted));
    }

    [TestMethod]
    public async Task FilterByCategory_ReturnsFilteredTodos()
    {
        await _client.PostAsJsonAsync("/api/todos", new { Title = "FCat", Priority = "Medium", Category = "TestCat" });
        var todos = await _client.GetFromJsonAsync<TodoItem[]>("/api/todos?category=testcat", TestJsonOptions.Default);
        Assert.IsTrue(todos!.Length > 0);
    }

    [TestMethod]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/todos/99999", new { Title = "N", Priority = "Low" });
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/todos/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
