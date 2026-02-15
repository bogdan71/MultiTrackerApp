using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class ItemEndpointsTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;
    private static string _testSlug = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory();
        _client = await _factory.CreateAuthenticatedClientAsync("itemtest@example.com", "TestPass123!");

        // Create a test category to hold items
        var catResp = await _client.PostAsJsonAsync("/api/categories", new { Name = "Item Test Cat", Slug = "item-test-cat", Icon = "🧪" });
        catResp.EnsureSuccessStatusCode();
        var cat = await catResp.Content.ReadFromJsonAsync<Category>(TestJsonOptions.Default);
        _testSlug = cat!.Slug;
    }

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    public async Task Unauthenticated_Returns401()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync($"/api/categories/{_testSlug}/items");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_ReturnsList()
    {
        var items = await _client.GetFromJsonAsync<Item[]>($"/api/categories/{_testSlug}/items", TestJsonOptions.Default);
        Assert.IsNotNull(items);
    }

    [TestMethod]
    public async Task CreateAndGet_ReturnsItem()
    {
        var response = await _client.PostAsJsonAsync($"/api/categories/{_testSlug}/items", new { Title = "Test Item", Description = "Desc" });
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Item>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Item", created.Title);
    }

    [TestMethod]
    public async Task Update_ReturnsNoContent()
    {
        var createResp = await _client.PostAsJsonAsync($"/api/categories/{_testSlug}/items", new { Title = "Update Me" });
        var created = await createResp.Content.ReadFromJsonAsync<Item>(TestJsonOptions.Default);

        var updateResp = await _client.PutAsJsonAsync($"/api/categories/{_testSlug}/items/{created!.Id}", new { Title = "Updated", Description = "New Desc", Status = "Completed" });
        Assert.AreEqual(HttpStatusCode.NoContent, updateResp.StatusCode);
    }

    [TestMethod]
    public async Task Delete_ReturnsNoContent()
    {
        var createResp = await _client.PostAsJsonAsync($"/api/categories/{_testSlug}/items", new { Title = "Delete Me" });
        var created = await createResp.Content.ReadFromJsonAsync<Item>(TestJsonOptions.Default);

        var deleteResp = await _client.DeleteAsync($"/api/categories/{_testSlug}/items/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [TestMethod]
    public async Task GetItems_NonExistentCategory_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/categories/non-existent-slug/items");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateItem_NonExistentCategory_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/categories/non-existent-slug/items", new { Title = "Test" });
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateItem_NonExistentCategory_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/categories/non-existent-slug/items/1", new { Title = "Test" });
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteItem_NonExistentCategory_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/categories/non-existent-slug/items/1");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteItem_NonExistentItem_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/categories/{_testSlug}/items/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
