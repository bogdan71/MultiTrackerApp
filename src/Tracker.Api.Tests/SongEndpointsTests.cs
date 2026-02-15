using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Api.Models;

namespace Tracker.Api.Tests;

[TestClass]
public class SongEndpointsTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        _factory = new CustomWebApplicationFactory();
        _client = await _factory.CreateAuthenticatedClientAsync("songtest@example.com", "TestPass123!");
    }

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    public async Task Unauthenticated_Returns401()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/songs");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAll_ReturnsEmptyList()
    {
        var songs = await _client.GetFromJsonAsync<Song[]>("/api/songs", TestJsonOptions.Default);
        Assert.IsNotNull(songs);
    }

    [TestMethod]
    public async Task CreateAndGet_ReturnsSong()
    {
        var response = await _client.PostAsJsonAsync("/api/songs", new { Title = "Test Song", Artist = "Artist", Album = "Album", Genre = "Pop", Status = "Upcoming" });
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);
        Assert.IsNotNull(created);
        Assert.AreEqual("Test Song", created.Title);

        var fetched = await _client.GetFromJsonAsync<Song>($"/api/songs/{created.Id}", TestJsonOptions.Default);
        Assert.AreEqual("Test Song", fetched!.Title);
    }

    [TestMethod]
    public async Task Update_ReturnsUpdatedSong()
    {
        var createResp = await _client.PostAsJsonAsync("/api/songs", new { Title = "Update Me", Artist = "A", Genre = "Rock", Status = "Upcoming" });
        var created = await createResp.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);

        var updateResp = await _client.PutAsJsonAsync($"/api/songs/{created!.Id}", new { Title = "Updated", Artist = "B", Album = "X", Genre = "Jazz", Status = "Completed" });
        Assert.AreEqual(HttpStatusCode.OK, updateResp.StatusCode);

        var updated = await updateResp.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);
        Assert.AreEqual("Updated", updated!.Title);
        Assert.AreEqual(TrackingStatus.Completed, updated.Status);
    }

    [TestMethod]
    public async Task Delete_ReturnsNoContent()
    {
        var createResp = await _client.PostAsJsonAsync("/api/songs", new { Title = "Delete Me", Artist = "A", Genre = "X", Status = "Upcoming" });
        var created = await createResp.Content.ReadFromJsonAsync<Song>(TestJsonOptions.Default);

        var deleteResp = await _client.DeleteAsync($"/api/songs/{created!.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [TestMethod]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/songs/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task FilterByStatus_ReturnsFilteredSongs()
    {
        await _client.PostAsJsonAsync("/api/songs", new { Title = "FS", Artist = "A", Genre = "X", Status = "InProgress" });
        var songs = await _client.GetFromJsonAsync<Song[]>("/api/songs?status=InProgress", TestJsonOptions.Default);
        Assert.IsTrue(songs!.All(s => s.Status == TrackingStatus.InProgress));
    }

    [TestMethod]
    public async Task FilterByGenre_ReturnsFilteredSongs()
    {
        await _client.PostAsJsonAsync("/api/songs", new { Title = "FG", Artist = "A", Genre = "Electronic", Status = "Upcoming" });
        var songs = await _client.GetFromJsonAsync<Song[]>("/api/songs?genre=electronic", TestJsonOptions.Default);
        Assert.IsTrue(songs!.Length > 0);
    }

    [TestMethod]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/songs/99999", new { Title = "N", Artist = "A", Genre = "X", Status = "Upcoming" });
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/songs/99999");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
