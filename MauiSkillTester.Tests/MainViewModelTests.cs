using MauiSkillTester.Models;
using MauiSkillTester.Services;
using MauiSkillTester.ViewModels;

namespace MauiSkillTester.Tests;

public class MainViewModelTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly IDatabaseService _databaseService;

    public MainViewModelTests()
    {
        SQLitePCL.Batteries_V2.Init();
        _testDbPath = Path.Combine(Path.GetTempPath(), $"ViewModelTestDb_{Guid.NewGuid():N}.db3");
        _databaseService = new SQLiteDatabaseService(_testDbPath);
    }

    public void Dispose()
    {
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task LoadDataAsync_ShouldPopulatePlaylistsAndSelectFirst()
    {
        // Arrange
        var viewModel = new MainViewModel(_databaseService);

        // Act
        await viewModel.LoadDataAsync();
        await viewModel.LoadVideosForSelectedPlaylistAsync();

        // Assert
        Assert.NotEmpty(viewModel.Playlists);
        Assert.NotNull(viewModel.SelectedPlaylist);
        Assert.True(viewModel.HasSelectedPlaylist);
        Assert.NotEmpty(viewModel.Videos);

    }

    [Fact]
    public async Task AddPlaylistCommand_ShouldAddNewPlaylist_AndClearFormInputs()
    {
        // Arrange
        var viewModel = new MainViewModel(_databaseService);
        await viewModel.LoadDataAsync();
        int initialCount = viewModel.Playlists.Count;

        viewModel.NewPlaylistName = "פלייליסט חדש מהבדיקה";
        viewModel.NewPlaylistDescription = "תיאור קצר לבדיקה";

        // Act
        Assert.True(viewModel.AddPlaylistCommand.CanExecute(null));
        viewModel.AddPlaylistCommand.Execute(null);
        await Task.Delay(100); // הפעולה אסינכרונית בזיכרון

        // Assert
        Assert.Equal(initialCount + 1, viewModel.Playlists.Count);
        Assert.NotNull(viewModel.SelectedPlaylist);
        Assert.Equal("פלייליסט חדש מהבדיקה", viewModel.SelectedPlaylist.Name);
        Assert.Empty(viewModel.NewPlaylistName);
        Assert.Empty(viewModel.NewPlaylistDescription);
    }

    [Fact]
    public async Task AddVideoCommand_ShouldAddNewVideo_WhenFormIsValid()
    {
        // Arrange
        var viewModel = new MainViewModel(_databaseService);
        await viewModel.LoadDataAsync();
        int initialVideoCount = viewModel.Videos.Count;

        viewModel.NewVideoTitle = "סרטון תרחיש בדיקה";
        viewModel.NewVideoUrl = "https://www.youtube.com/watch?v=unittest";
        viewModel.NewVideoChannel = "ערוץ MAUI";
        viewModel.NewVideoDuration = 10;
        viewModel.NewVideoNotes = "הערה";

        // Act
        Assert.True(viewModel.AddVideoCommand.CanExecute(null));
        viewModel.AddVideoCommand.Execute(null);
        await Task.Delay(300);

        // Assert
        Assert.Equal(initialVideoCount + 1, viewModel.Videos.Count);
        Assert.Equal("סרטון תרחיש בדיקה", viewModel.Videos[0].Title);
        Assert.Empty(viewModel.NewVideoTitle);
        Assert.Empty(viewModel.NewVideoUrl);
    }

    [Fact]
    public async Task SearchQuery_ShouldFilterVideosCorrectly()
    {
        // Arrange
        var viewModel = new MainViewModel(_databaseService);
        await viewModel.LoadDataAsync();

        // הוספת סרטון ספציפי לחיפוש
        viewModel.NewVideoTitle = "מדריך יחידה מתקדם";
        viewModel.NewVideoUrl = "https://youtube.com/watch?v=searchtest";
        viewModel.NewVideoChannel = "ערוץ בדיקות";
        viewModel.AddVideoCommand.Execute(null);
        await Task.Delay(300);

        // Act
        viewModel.SearchQuery = "מתקדם";
        await Task.Delay(300);


        // Assert
        Assert.Single(viewModel.Videos);
        Assert.Contains("מתקדם", viewModel.Videos[0].Title);
    }

    [Fact]
    public async Task ShowOnlyUnwatched_ShouldFilterOutWatchedVideos()
    {
        // Arrange
        var viewModel = new MainViewModel(_databaseService);
        await viewModel.LoadDataAsync();

        // Act
        viewModel.ShowOnlyUnwatched = true;
        await Task.Delay(100);

        // Assert
        Assert.All(viewModel.Videos, video => Assert.False(video.IsWatched));
    }

    [Fact]
    public async Task LoadVideosForSelectedPlaylistAsync_ShouldUpdateCurrentPlaylistStats()
    {
        // Arrange
        var viewModel = new MainViewModel(_databaseService);
        await viewModel.LoadDataAsync();

        // Act
        await viewModel.LoadVideosForSelectedPlaylistAsync();

        // Assert
        Assert.NotNull(viewModel.CurrentPlaylistStats);
        Assert.True(viewModel.HasStats);
        Assert.True(viewModel.CurrentPlaylistStats.TotalVideos > 0);
    }
}
