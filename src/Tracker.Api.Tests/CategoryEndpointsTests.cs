using System.Net;
using System.Net.Http.Json;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class CategoryEndpointsTests
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
    public async Task GetAll_ReturnsOkWithCategories()
    {
        var response = await _client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();

        var categories = await response.Content.ReadFromJsonAsync<List<Category>>(TestJsonOptions.Default);
        Assert.IsNotNull(categories);
        Assert.IsTrue(categories.Count >= 2, "Should have at least the 2 seeded categories");
    }

    [TestMethod]
    public async Task GetBySlug_ExistingCategory_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/categories/apps");
        response.EnsureSuccessStatusCode();

        var category = await response.Content.ReadFromJsonAsync<Category>(TestJsonOptions.Default);
        Assert.IsNotNull(category);
        Assert.AreEqual("Apps", category.Name);
        Assert.AreEqual("apps", category.Slug);
    }

    [TestMethod]
    public async Task GetBySlug_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/categories/nonexistent-slug");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_ReturnsCreated()
    {
        var newCategory = new Category
        {
            Name = "Games",
            Slug = "games",
            Icon = "🎮",
            Description = "Video games"
        };

        var response = await _client.PostAsJsonAsync("/api/categories", newCategory, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Category>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Games", created.Name);
        Assert.AreEqual("games", created.Slug);
    }

    [TestMethod]
    public async Task Delete_ExistingCategory_ReturnsNoContent()
    {
        var newCategory = new Category { Name = "Temp", Slug = "temp-delete" };
        var createResponse = await _client.PostAsJsonAsync("/api/categories", newCategory, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Category>(TestJsonOptions.Default);

        var response = await _client.DeleteAsync($"/api/categories/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/categories/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
