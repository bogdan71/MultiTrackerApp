using System.Net;
using System.Net.Http.Json;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class TodoEndpointsTests
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
    public async Task GetAll_ReturnsOkWithTodos()
    {
        var response = await _client.GetAsync("/api/todos");
        response.EnsureSuccessStatusCode();

        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>(TestJsonOptions.Default);
        Assert.IsNotNull(todos);
        Assert.IsTrue(todos.Count >= 3, "Should have at least the 3 seeded todos");
    }

    [TestMethod]
    public async Task GetById_ExistingTodo_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/todos/1");
        response.EnsureSuccessStatusCode();

        var todo = await response.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsNotNull(todo);
        Assert.AreEqual("Set up home theater for Avatar 3", todo.Title);
    }

    [TestMethod]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/todos/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_ReturnsCreated()
    {
        var newTodo = new TodoItem
        {
            Title = "Test Todo",
            Description = "Test Description",
            Priority = Priority.High,
            Category = "Testing"
        };

        var response = await _client.PostAsJsonAsync("/api/todos", newTodo, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Todo", created.Title);
        Assert.AreEqual(Priority.High, created.Priority);
        Assert.IsTrue(created.Id > 0);
    }

    [TestMethod]
    public async Task Update_ExistingTodo_ReturnsOk()
    {
        var newTodo = new TodoItem { Title = "Update Me", Priority = Priority.Low };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", newTodo, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);

        created!.Title = "Updated Todo";
        created.Priority = Priority.Critical;
        var response = await _client.PutAsJsonAsync($"/api/todos/{created.Id}", created, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.AreEqual("Updated Todo", updated!.Title);
        Assert.AreEqual(Priority.Critical, updated.Priority);
    }

    [TestMethod]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var todo = new TodoItem { Title = "Ghost" };
        var response = await _client.PutAsJsonAsync("/api/todos/9999", todo, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Toggle_FlipsIsCompleted()
    {
        var newTodo = new TodoItem { Title = "Toggle Me", IsCompleted = false };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", newTodo, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsFalse(created!.IsCompleted);

        // Toggle to completed
        var toggleResponse = await _client.PatchAsync($"/api/todos/{created.Id}/toggle", null);
        Assert.AreEqual(HttpStatusCode.OK, toggleResponse.StatusCode);
        var toggled = await toggleResponse.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsTrue(toggled!.IsCompleted);

        // Toggle back to incomplete
        var toggleResponse2 = await _client.PatchAsync($"/api/todos/{created.Id}/toggle", null);
        Assert.AreEqual(HttpStatusCode.OK, toggleResponse2.StatusCode);
        var toggled2 = await toggleResponse2.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);
        Assert.IsFalse(toggled2!.IsCompleted);
    }

    [TestMethod]
    public async Task Toggle_NonExistent_ReturnsNotFound()
    {
        var response = await _client.PatchAsync("/api/todos/9999/toggle", null);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_ExistingTodo_ReturnsNoContent()
    {
        var newTodo = new TodoItem { Title = "Delete Me" };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", newTodo, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>(TestJsonOptions.Default);

        var response = await _client.DeleteAsync($"/api/todos/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/todos/{created.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/todos/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task FilterByPriority_ReturnsFilteredTodos()
    {
        var response = await _client.GetAsync("/api/todos?priority=High");
        response.EnsureSuccessStatusCode();

        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>(TestJsonOptions.Default);
        Assert.IsNotNull(todos);
        Assert.IsTrue(todos.All(t => t.Priority == Priority.High));
    }

    [TestMethod]
    public async Task FilterByCompleted_ReturnsFilteredTodos()
    {
        var response = await _client.GetAsync("/api/todos?completed=true");
        response.EnsureSuccessStatusCode();

        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>(TestJsonOptions.Default);
        Assert.IsNotNull(todos);
        Assert.IsTrue(todos.All(t => t.IsCompleted));
    }

    [TestMethod]
    public async Task FilterByCategory_ReturnsFilteredTodos()
    {
        var response = await _client.GetAsync("/api/todos?category=Entertainment");
        response.EnsureSuccessStatusCode();

        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>(TestJsonOptions.Default);
        Assert.IsNotNull(todos);
        Assert.IsTrue(todos.All(t => t.Category != null && t.Category.Contains("Entertainment", StringComparison.OrdinalIgnoreCase)));
    }
}
