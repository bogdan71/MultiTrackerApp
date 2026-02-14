using System.Net;
using System.Net.Http.Json;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class BookEndpointsTests
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
    public async Task GetAll_ReturnsOkWithBooks()
    {
        var response = await _client.GetAsync("/api/books");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<Book>>(TestJsonOptions.Default);
        Assert.IsNotNull(books);
        Assert.IsTrue(books.Count >= 3, "Should have at least the 3 seeded books");
    }

    [TestMethod]
    public async Task GetById_ExistingBook_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/books/1");
        response.EnsureSuccessStatusCode();

        var book = await response.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);
        Assert.IsNotNull(book);
        Assert.AreEqual("The Winds of Winter", book.Title);
    }

    [TestMethod]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/books/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_ReturnsCreated()
    {
        var newBook = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Genre = "Test Genre",
            Status = TrackingStatus.Upcoming
        };

        var response = await _client.PostAsJsonAsync("/api/books", newBook, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Book", created.Title);
        Assert.IsTrue(created.Id > 0);
    }

    [TestMethod]
    public async Task Update_ExistingBook_ReturnsOk()
    {
        var newBook = new Book { Title = "Update Me", Author = "Author" };
        var createResponse = await _client.PostAsJsonAsync("/api/books", newBook, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);

        created!.Title = "Updated Title";
        var response = await _client.PutAsJsonAsync($"/api/books/{created.Id}", created, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);
        Assert.AreEqual("Updated Title", updated!.Title);
    }

    [TestMethod]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var book = new Book { Title = "Ghost" };
        var response = await _client.PutAsJsonAsync("/api/books/9999", book, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_ExistingBook_ReturnsNoContent()
    {
        var newBook = new Book { Title = "Delete Me" };
        var createResponse = await _client.PostAsJsonAsync("/api/books", newBook, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Book>(TestJsonOptions.Default);

        var response = await _client.DeleteAsync($"/api/books/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/books/{created.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/books/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task FilterByStatus_ReturnsFilteredBooks()
    {
        var response = await _client.GetAsync("/api/books?status=Completed");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<Book>>(TestJsonOptions.Default);
        Assert.IsNotNull(books);
        Assert.IsTrue(books.All(b => b.Status == TrackingStatus.Completed));
    }

    [TestMethod]
    public async Task FilterByGenre_ReturnsFilteredBooks()
    {
        var response = await _client.GetAsync("/api/books?genre=Sci-Fi");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<Book>>(TestJsonOptions.Default);
        Assert.IsNotNull(books);
        Assert.IsTrue(books.All(b => b.Genre != null && b.Genre.Contains("Sci-Fi", StringComparison.OrdinalIgnoreCase)));
    }
}
