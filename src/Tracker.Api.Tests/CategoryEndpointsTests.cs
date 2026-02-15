using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class CategoryEndpointsTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory();
        _client = await _factory.CreateAuthenticatedClientAsync("cattest@example.com", "TestPass123!");
    }

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    public async Task Unauthenticated_Returns401()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/categories");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_ReturnsList()
    {
        var categories = await _client.GetFromJsonAsync<Category[]>("/api/categories", TestJsonOptions.Default);
        Assert.IsNotNull(categories);
    }

    [TestMethod]
    public async Task CreateAndGetBySlug_ReturnsCategory()
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new { Name = "My Category", Slug = "my-category", Icon = "📁" });
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Category>(TestJsonOptions.Default);
        Assert.AreEqual("My Category", created!.Name);

        var fetched = await _client.GetFromJsonAsync<Category>($"/api/categories/{created.Slug}", TestJsonOptions.Default);
        Assert.AreEqual("My Category", fetched!.Name);
    }

    [TestMethod]
    public async Task Delete_ExistingCategory_ReturnsNoContent()
    {
        var createResp = await _client.PostAsJsonAsync("/api/categories", new { Name = "Delete Cat", Slug = "delete-cat" });
        var created = await createResp.Content.ReadFromJsonAsync<Category>(TestJsonOptions.Default);

        var deleteResp = await _client.DeleteAsync($"/api/categories/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [TestMethod]
    public async Task GetBySlug_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/categories/non-existent-slug");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/categories/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
