using System.Net;
using System.Net.Http.Json;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class ItemEndpointsTests
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
    public async Task GetItemsByCategory_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/categories/apps/items");
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<Item>>(TestJsonOptions.Default);
        Assert.IsNotNull(items);
        Assert.IsTrue(items.Count >= 2, "Should have at least the 2 seeded items in 'apps' category");
    }

    [TestMethod]
    public async Task GetItemsByCategory_NonExistentCategory_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/categories/nonexistent/items");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateItem_ReturnsCreated()
    {
        var newItem = new Item
        {
            Title = "Test App",
            Description = "A test application",
            Status = "Active",
            Properties = "{\"Platform\":\"Web\"}"
        };

        var response = await _client.PostAsJsonAsync("/api/categories/apps/items", newItem, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Item>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test App", created.Title);
        Assert.IsTrue(created.Id > 0);
    }

    [TestMethod]
    public async Task CreateItem_NonExistentCategory_ReturnsNotFound()
    {
        var newItem = new Item { Title = "Ghost Item" };
        var response = await _client.PostAsJsonAsync("/api/categories/nonexistent/items", newItem, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateItem_ReturnsNoContent()
    {
        var newItem = new Item { Title = "Update Me", Description = "Original" };
        var createResponse = await _client.PostAsJsonAsync("/api/categories/apps/items", newItem, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Item>(TestJsonOptions.Default);

        var updatedItem = new Item
        {
            Title = "Updated App",
            Description = "Updated description",
            Status = "Archived"
        };

        var response = await _client.PutAsJsonAsync($"/api/categories/apps/items/{created!.Id}", updatedItem, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateItem_NonExistentCategory_ReturnsNotFound()
    {
        var item = new Item { Title = "Ghost" };
        var response = await _client.PutAsJsonAsync("/api/categories/nonexistent/items/1", item, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateItem_NonExistentItem_ReturnsNotFound()
    {
        var item = new Item { Title = "Ghost" };
        var response = await _client.PutAsJsonAsync("/api/categories/apps/items/9999", item, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteItem_ReturnsNoContent()
    {
        var newItem = new Item { Title = "Delete Me" };
        var createResponse = await _client.PostAsJsonAsync("/api/categories/apps/items", newItem, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Item>(TestJsonOptions.Default);

        var response = await _client.DeleteAsync($"/api/categories/apps/items/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteItem_NonExistentCategory_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/categories/nonexistent/items/1");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteItem_NonExistentItem_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/categories/apps/items/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
