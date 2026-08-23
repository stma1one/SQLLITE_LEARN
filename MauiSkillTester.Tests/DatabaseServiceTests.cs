using MauiSkillTester.Models;
using MauiSkillTester.Services;
using SQLite;

namespace MauiSkillTester.Tests;

public class DatabaseServiceTests : IDisposable
{
    private readonly string _testDbPath;

    public DatabaseServiceTests()
    {
        // אתחול ספריית ה-C הגולמית של SQLite עבור סביבת הבדיקות
        SQLitePCL.Batteries_V2.Init();

        // יצירת קובץ SQLite זמני וייחודי לכל בדיקת יחידה (סביבה סטרילית)
        _testDbPath = Path.Combine(Path.GetTempPath(), $"TestDb_{Guid.NewGuid():N}.db3");
    }

    public void Dispose()
    {
        // מחיקת קובץ הבדיקה בסיום הרצת הבדיקה
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // להתעלם מנעילת קובץ זמנית במידה וקיים
            }
        }
    }

    private IDatabaseService CreateService()
    {
        return new SQLiteDatabaseService(_testDbPath);
    }

    [Fact]
    public async Task InitializeAsync_ShouldCreateTablesAndSeedInitialData()
    {
        // Arrange
        var service = CreateService();

        // Act
        var playlists = await service.GetPlaylistsAsync();
        var videos = await service.GetAllVideosAsync();

        // Assert
        Assert.NotNull(playlists);
        Assert.NotEmpty(playlists);
        Assert.True(playlists.Count >= 2);
        Assert.NotEmpty(videos);
    }

    [Fact]
    public async Task SavePlaylistAsync_ShouldInsertNewPlaylist_AndAssignId()
    {
        // Arrange
        var service = CreateService();
        var newPlaylist = new Playlist
        {
            Name = "פלייליסט בדיקה",
            Description = "תיאור לבדיקה",
            CreatedAt = DateTime.Now
        };

        // Act
        int result = await service.SavePlaylistAsync(newPlaylist);
        var fetched = await service.GetPlaylistByIdAsync(newPlaylist.Id);

        // Assert
        Assert.True(result > 0);
        Assert.True(newPlaylist.Id > 0);
        Assert.NotNull(fetched);
        Assert.Equal("פלייליסט בדיקה", fetched.Name);
    }

    [Fact]
    public async Task SavePlaylistAsync_ShouldUpdateExistingPlaylist()
    {
        // Arrange
        var service = CreateService();
        var playlists = await service.GetPlaylistsAsync();
        var targetPlaylist = playlists.First();
        targetPlaylist.Name = "שם פלייליסט מעודכן";

        // Act
        await service.SavePlaylistAsync(targetPlaylist);
        var updated = await service.GetPlaylistByIdAsync(targetPlaylist.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("שם פלייליסט מעודכן", updated.Name);
    }

    [Fact]
    public async Task SaveVideoAsync_ShouldInsertVideoWithForeignKey()
    {
        // Arrange
        var service = CreateService();
        var playlists = await service.GetPlaylistsAsync();
        var targetPlaylist = playlists.First();

        var video = new VideoItem
        {
            PlaylistId = targetPlaylist.Id,
            Title = "סרטון חדש לבדיקה",
            Url = "https://www.youtube.com/watch?v=test",
            ChannelName = "ערוץ בדיקה",
            DurationMinutes = 15,
            IsWatched = false,
            Notes = "הערת בדיקה",
            CreatedAt = DateTime.Now
        };

        // Act
        int result = await service.SaveVideoAsync(video);
        var playlistVideos = await service.GetVideosByPlaylistAsync(targetPlaylist.Id);

        // Assert
        Assert.True(result > 0);
        Assert.True(video.Id > 0);
        Assert.Contains(playlistVideos, v => v.Id == video.Id && v.Title == "סרטון חדש לבדיקה");
    }

    [Fact]
    public async Task ToggleWatchedAsync_ShouldInvertIsWatchedState()
    {
        // Arrange
        var service = CreateService();
        var videos = await service.GetAllVideosAsync();
        var video = videos.First();
        bool initialWatched = video.IsWatched;

        // Act
        await service.ToggleWatchedAsync(video);
        var updatedVideo = await service.GetVideoByIdAsync(video.Id);

        // Assert
        Assert.NotNull(updatedVideo);
        Assert.Equal(!initialWatched, updatedVideo.IsWatched);
    }

    [Fact]
    public async Task DeletePlaylistAsync_ShouldCascadeDeleteVideos()
    {
        // Arrange
        var service = CreateService();
        var playlists = await service.GetPlaylistsAsync();
        var targetPlaylist = playlists.First();

        // Act
        await service.DeletePlaylistAsync(targetPlaylist);
        var deletedPlaylist = await service.GetPlaylistByIdAsync(targetPlaylist.Id);
        var remainingVideos = await service.GetVideosByPlaylistAsync(targetPlaylist.Id);

        // Assert
        Assert.Null(deletedPlaylist);
        Assert.Empty(remainingVideos);
    }

    [Fact]
    public async Task GetPlaylistStatsAsync_ShouldCalculateCorrectTotalsAndCompletionPercentage()
    {
        // Arrange
        var service = CreateService();
        var playlists = await service.GetPlaylistsAsync();
        var playlist = playlists.First(p => p.Name.Contains(".NET MAUI")); // Seeded with 2 videos (1 watched, 1 unwatched, durations: 12 + 18 = 30)

        // Act
        var stats = await service.GetPlaylistStatsAsync(playlist.Id);

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(playlist.Id, stats.PlaylistId);
        Assert.Equal(2, stats.TotalVideos);
        Assert.Equal(1, stats.WatchedVideos);
        Assert.Equal(1, stats.UnwatchedVideos);
        Assert.Equal(30, stats.TotalDurationMinutes);
        Assert.Equal(50.0, stats.CompletionPercentage);
        Assert.Equal("30 דקות", stats.FormattedTotalDuration);
    }

    [Fact]
    public async Task GetPlaylistStatsAsync_OnEmptyPlaylist_ShouldReturnZeroes_WithoutNullErrors()
    {
        // Arrange
        var service = CreateService();
        var emptyPlaylist = new Playlist { Name = "פלייליסט ריק", CreatedAt = DateTime.Now };
        await service.SavePlaylistAsync(emptyPlaylist);

        // Act
        var stats = await service.GetPlaylistStatsAsync(emptyPlaylist.Id);

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(0, stats.TotalVideos);
        Assert.Equal(0, stats.WatchedVideos);
        Assert.Equal(0, stats.UnwatchedVideos);
        Assert.Equal(0, stats.TotalDurationMinutes);
        Assert.Equal(0.0, stats.CompletionPercentage);
    }

    [Fact]
    public async Task GetFilteredVideosAsync_WithSearchQuery_ShouldFilterByTitleChannelOrNotes()
    {
        // Arrange
        var service = CreateService();
        var playlists = await service.GetPlaylistsAsync();
        var playlist = playlists.First(p => p.Name.Contains(".NET MAUI"));

        // Act - חיפוש לפי שם ערוץ
        var resultsByChannel = await service.GetFilteredVideosAsync(playlist.Id, searchQuery: "Gerald");

        // Assert
        Assert.Single(resultsByChannel);
        Assert.Equal("Gerald Versluis", resultsByChannel[0].ChannelName);
    }

    [Fact]
    public async Task GetFilteredVideosAsync_WithOnlyUnwatched_ShouldFilterOutWatchedVideos()
    {
        // Arrange
        var service = CreateService();
        var playlists = await service.GetPlaylistsAsync();
        var playlist = playlists.First(p => p.Name.Contains(".NET MAUI"));

        // Act
        var results = await service.GetFilteredVideosAsync(playlist.Id, onlyUnwatched: true);

        // Assert
        Assert.All(results, v => Assert.False(v.IsWatched));
    }

    [Fact]
    public async Task GetFilteredVideosAsync_WithDifferentSortOptions_ShouldOrderResultsCorrectly()
    {
        // Arrange
        var service = CreateService();
        var playlists = await service.GetPlaylistsAsync();
        var playlist = playlists.First(p => p.Name.Contains(".NET MAUI"));

        // Act - מיון לפי הקצרים ביותר תחילה
        var shortestFirst = await service.GetFilteredVideosAsync(playlist.Id, sortBy: VideoSortOption.ShortestFirst);

        // Assert
        Assert.Equal(2, shortestFirst.Count);
        Assert.True(shortestFirst[0].DurationMinutes <= shortestFirst[1].DurationMinutes);

        // Act - מיון לפי הארוכים ביותר תחילה
        var longestFirst = await service.GetFilteredVideosAsync(playlist.Id, sortBy: VideoSortOption.LongestFirst);

        // Assert
        Assert.True(longestFirst[0].DurationMinutes >= longestFirst[1].DurationMinutes);
    }

    [Fact]
    public async Task GetFilteredVideosAsync_WithSpecialCharacters_ShouldEscapeAndFindExactLiteralMatch()
    {
        // Arrange
        var service = CreateService();
        var playlists = await service.GetPlaylistsAsync();
        var playlist = playlists.First();

        var specialVideo = new VideoItem
        {
            PlaylistId = playlist.Id,
            Title = "מדריך של 100% הצלחה ב-C#",
            Url = "https://youtube.com/watch?v=100percent",
            ChannelName = "SpecialCode",
            DurationMinutes = 15,
            CreatedAt = DateTime.Now
        };
        await service.SaveVideoAsync(specialVideo);

        // Act - חיפוש מילולי של "100%"
        var results = await service.GetFilteredVideosAsync(playlist.Id, searchQuery: "100%");

        // Assert
        Assert.Single(results);
        Assert.Contains("100%", results[0].Title);
    }
}
