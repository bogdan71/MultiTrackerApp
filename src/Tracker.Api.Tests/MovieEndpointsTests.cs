using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class MovieEndpointsTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory();
        _client = await _factory.CreateAuthenticatedClientAsync("movietest@example.com", "TestPass123!");
    }

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    public async Task Unauthenticated_Returns401()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/movies");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_ReturnsEmptyList()
    {
        var movies = await _client.GetFromJsonAsync<Movie[]>("/api/movies", TestJsonOptions.Default);
        Assert.IsNotNull(movies);
    }

    [TestMethod]
    public async Task CreateAndGet_ReturnsMovie()
    {
        var newMovie = new { Title = "Test Movie", Director = "Director", Genre = "Action", Status = "Upcoming" };
        var response = await _client.PostAsJsonAsync("/api/movies", newMovie);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Movie", created.Title);

        var fetched = await _client.GetFromJsonAsync<Movie>($"/api/movies/{created.Id}", TestJsonOptions.Default);
        Assert.AreEqual("Test Movie", fetched!.Title);
    }

    [TestMethod]
    public async Task Update_ReturnsUpdatedMovie()
    {
        var createResp = await _client.PostAsJsonAsync("/api/movies", new { Title = "Update Me", Director = "D", Genre = "Drama", Status = "Upcoming" });
        var created = await createResp.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);

        var updateResp = await _client.PutAsJsonAsync($"/api/movies/{created!.Id}", new { Title = "Updated", Director = "D2", Genre = "Comedy", Status = "Completed" });
        Assert.AreEqual(HttpStatusCode.OK, updateResp.StatusCode);

        var updated = await updateResp.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);
        Assert.AreEqual("Updated", updated!.Title);
        Assert.AreEqual(TrackingStatus.Completed, updated.Status);
    }

    [TestMethod]
    public async Task Delete_ReturnsNoContent()
    {
        var createResp = await _client.PostAsJsonAsync("/api/movies", new { Title = "Delete Me", Director = "D", Genre = "X", Status = "Upcoming" });
        var created = await createResp.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);

        var deleteResp = await _client.DeleteAsync($"/api/movies/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [TestMethod]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/movies/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task FilterByStatus_ReturnsFilteredMovies()
    {
        await _client.PostAsJsonAsync("/api/movies", new { Title = "FS", Director = "D", Genre = "X", Status = "InProgress" });
        var movies = await _client.GetFromJsonAsync<Movie[]>("/api/movies?status=InProgress", TestJsonOptions.Default);
        Assert.IsTrue(movies!.All(m => m.Status == TrackingStatus.InProgress));
    }

    [TestMethod]
    public async Task FilterByGenre_ReturnsFilteredMovies()
    {
        await _client.PostAsJsonAsync("/api/movies", new { Title = "FG", Director = "D", Genre = "Thriller", Status = "Upcoming" });
        var movies = await _client.GetFromJsonAsync<Movie[]>("/api/movies?genre=thriller", TestJsonOptions.Default);
        Assert.IsTrue(movies!.Length > 0);
    }

    [TestMethod]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/movies/99999", new { Title = "N", Director = "D", Genre = "X", Status = "Upcoming" });
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/movies/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
