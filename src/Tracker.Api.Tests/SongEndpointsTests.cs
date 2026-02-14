using System.Net;
using System.Net.Http.Json;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class SongEndpointsTests
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
    public async Task GetAll_ReturnsOkWithSongs()
    {
        var response = await _client.GetAsync("/api/songs");
        response.EnsureSuccessStatusCode();

        var songs = await response.Content.ReadFromJsonAsync<List<Song>>(TestJsonOptions.Default);
        Assert.IsNotNull(songs);
        Assert.IsTrue(songs.Count >= 3, "Should have at least the 3 seeded songs");
    }

    [TestMethod]
    public async Task GetById_ExistingSong_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/songs/1");
        response.EnsureSuccessStatusCode();

        var song = await response.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);
        Assert.IsNotNull(song);
        Assert.AreEqual("Midnight Rain", song.Title);
    }

    [TestMethod]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/songs/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_ReturnsCreated()
    {
        var newSong = new Song
        {
            Title = "Test Song",
            Artist = "Test Artist",
            Album = "Test Album",
            Genre = "Rock",
            Status = TrackingStatus.InProgress
        };

        var response = await _client.PostAsJsonAsync("/api/songs", newSong, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Song", created.Title);
        Assert.IsTrue(created.Id > 0);
    }

    [TestMethod]
    public async Task Update_ExistingSong_ReturnsOk()
    {
        var newSong = new Song { Title = "Update Me", Artist = "Artist" };
        var createResponse = await _client.PostAsJsonAsync("/api/songs", newSong, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);

        created!.Title = "Updated Song";
        var response = await _client.PutAsJsonAsync($"/api/songs/{created.Id}", created, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);
        Assert.AreEqual("Updated Song", updated!.Title);
    }

    [TestMethod]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var song = new Song { Title = "Ghost" };
        var response = await _client.PutAsJsonAsync("/api/songs/9999", song, TestJsonOptions.Default);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_ExistingSong_ReturnsNoContent()
    {
        var newSong = new Song { Title = "Delete Me" };
        var createResponse = await _client.PostAsJsonAsync("/api/songs", newSong, TestJsonOptions.Default);
        var created = await createResponse.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);

        var response = await _client.DeleteAsync($"/api/songs/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/songs/{created.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/songs/9999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task FilterByStatus_ReturnsFilteredSongs()
    {
        var response = await _client.GetAsync("/api/songs?status=Completed");
        response.EnsureSuccessStatusCode();

        var songs = await response.Content.ReadFromJsonAsync<List<Song>>(TestJsonOptions.Default);
        Assert.IsNotNull(songs);
        Assert.IsTrue(songs.All(s => s.Status == TrackingStatus.Completed));
    }

    [TestMethod]
    public async Task FilterByGenre_ReturnsFilteredSongs()
    {
        var response = await _client.GetAsync("/api/songs?genre=Pop");
        response.EnsureSuccessStatusCode();

        var songs = await response.Content.ReadFromJsonAsync<List<Song>>(TestJsonOptions.Default);
        Assert.IsNotNull(songs);
        Assert.IsTrue(songs.All(s => s.Genre != null && s.Genre.Contains("Pop", StringComparison.OrdinalIgnoreCase)));
    }
}
