using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class BookEndpointsTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory();
        _client = await _factory.CreateAuthenticatedClientAsync();
    }

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    public async Task Unauthenticated_Returns401()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/books");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_ReturnsEmptyList()
    {
        var books = await _client.GetFromJsonAsync<Book[]>("/api/books", TestJsonOptions.Default);
        Assert.IsNotNull(books);
    }

    [TestMethod]
    public async Task CreateAndGet_ReturnsBook()
    {
        var newBook = new { Title = "Test Book", Author = "Author", Genre = "Sci-Fi", Status = "Upcoming" };
        var response = await _client.PostAsJsonAsync("/api/books", newBook);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Book", created.Title);
        Assert.AreEqual(TrackingStatus.Upcoming, created.Status);

        var fetched = await _client.GetFromJsonAsync<Book>($"/api/books/{created.Id}", TestJsonOptions.Default);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("Test Book", fetched.Title);
    }

    [TestMethod]
    public async Task Update_ReturnsUpdatedBook()
    {
        var newBook = new { Title = "Update Me", Author = "A", Genre = "Drama", Status = "Upcoming" };
        var createResp = await _client.PostAsJsonAsync("/api/books", newBook);
        var created = await createResp.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);

        var update = new { Title = "Updated Title", Author = "B", Genre = "Comedy", Status = "Completed" };
        var updateResp = await _client.PutAsJsonAsync($"/api/books/{created!.Id}", update);
        Assert.AreEqual(HttpStatusCode.OK, updateResp.StatusCode);

        var updated = await updateResp.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);
        Assert.AreEqual("Updated Title", updated!.Title);
        Assert.AreEqual(TrackingStatus.Completed, updated.Status);
    }

    [TestMethod]
    public async Task Delete_ReturnsNoContent()
    {
        var newBook = new { Title = "Delete Me", Author = "A", Genre = "X", Status = "Upcoming" };
        var createResp = await _client.PostAsJsonAsync("/api/books", newBook);
        var created = await createResp.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);

        var deleteResp = await _client.DeleteAsync($"/api/books/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var getResp = await _client.GetAsync($"/api/books/{created.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResp.StatusCode);
    }

    [TestMethod]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/books/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task FilterByStatus_ReturnsFilteredBooks()
    {
        await _client.PostAsJsonAsync("/api/books", new { Title = "Filter Status", Author = "A", Genre = "X", Status = "InProgress" });
        var books = await _client.GetFromJsonAsync<Book[]>("/api/books?status=InProgress", TestJsonOptions.Default);
        Assert.IsNotNull(books);
        Assert.IsTrue(books.All(b => b.Status == TrackingStatus.InProgress));
    }

    [TestMethod]
    public async Task FilterByGenre_ReturnsFilteredBooks()
    {
        await _client.PostAsJsonAsync("/api/books", new { Title = "Genre Book", Author = "A", Genre = "Fantasy", Status = "Upcoming" });
        var books = await _client.GetFromJsonAsync<Book[]>("/api/books?genre=fantasy", TestJsonOptions.Default);
        Assert.IsNotNull(books);
        Assert.IsTrue(books.Length > 0);
    }

    [TestMethod]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var update = new { Title = "Nope", Author = "A", Genre = "X", Status = "Upcoming" };
        var response = await _client.PutAsJsonAsync("/api/books/99999", update);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/books/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
