using System.Net;
using System.Net.Http.Json;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class MovieEndpointsTests
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
    public async Task GetAll_ReturnsOkWithMovies()
    {
        var response = await _client.GetAsync("/api/movies");
        response.EnsureSuccessStatusCode();

        var movies = await response.Content.ReadFromJsonAsync<List<Movie>>(TestJsonOptions.Default);
        Assert.IsNotNull(movies);
        Assert.IsTrue(movies.Count >= 3, "Should have at least the 3 seeded movies");
    }

    [TestMethod]
    public async Task GetById_ExistingMovie_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/movies/1");
        response.EnsureSuccessStatusCode();

        var movie = await response.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);
        Assert.IsNotNull(movie);
        Assert.AreEqual("Avatar 3", movie.Title);
    }

    [TestMethod]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/movies/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_ReturnsCreated()
    {
        var newMovie = new Movie
        {
            Title = "Test Movie",
            Director = "Test Director",
            Genre = "Action",
            Status = TrackingStatus.Upcoming
        };

        var response = await _client.PostAsJsonAsync("/api/movies", newMovie, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Movie", created.Title);
        Assert.IsTrue(created.Id > 0);
    }

    [TestMethod]
    public async Task Update_ExistingMovie_ReturnsOk()
    {
        var newMovie = new Movie { Title = "Update Me", Director = "Dir" };
        var createResponse = await _client.PostAsJsonAsync("/api/movies", newMovie, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);

        created!.Title = "Updated Movie";
        var response = await _client.PutAsJsonAsync($"/api/movies/{created.Id}", created, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);
        Assert.AreEqual("Updated Movie", updated!.Title);
    }

    [TestMethod]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var movie = new Movie { Title = "Ghost" };
        var response = await _client.PutAsJsonAsync("/api/movies/9999", movie, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_ExistingMovie_ReturnsNoContent()
    {
        var newMovie = new Movie { Title = "Delete Me" };
        var createResponse = await _client.PostAsJsonAsync("/api/movies", newMovie, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Movie>(TestJsonOptions.Default);

        var response = await _client.DeleteAsync($"/api/movies/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/movies/{created.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/movies/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task FilterByStatus_ReturnsFilteredMovies()
    {
        var response = await _client.GetAsync("/api/movies?status=Completed");
        response.EnsureSuccessStatusCode();

        var movies = await response.Content.ReadFromJsonAsync<List<Movie>>(TestJsonOptions.Default);
        Assert.IsNotNull(movies);
        Assert.IsTrue(movies.All(m => m.Status == TrackingStatus.Completed));
    }

    [TestMethod]
    public async Task FilterByGenre_ReturnsFilteredMovies()
    {
        var response = await _client.GetAsync("/api/movies?genre=Sci-Fi");
        response.EnsureSuccessStatusCode();

        var movies = await response.Content.ReadFromJsonAsync<List<Movie>>(TestJsonOptions.Default);
        Assert.IsNotNull(movies);
        Assert.IsTrue(movies.All(m => m.Genre != null && m.Genre.Contains("Sci-Fi", StringComparison.OrdinalIgnoreCase)));
    }
}
