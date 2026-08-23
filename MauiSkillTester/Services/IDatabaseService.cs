using MauiSkillTester.Models;

namespace MauiSkillTester.Services;

/// <summary>
/// חוזה שירות בסיס הנתונים לביצוע פעולות CRUD אסינכרוניות
/// </summary>
public interface IDatabaseService
{
    // אתחול
    Task InitializeAsync();

    // פלייליסטים (Playlists)
    Task<List<Playlist>> GetPlaylistsAsync();
    Task<Playlist?> GetPlaylistByIdAsync(int id);
    Task<int> SavePlaylistAsync(Playlist playlist);
    Task<int> DeletePlaylistAsync(Playlist playlist);

    // סרטונים (Videos)
    Task<List<VideoItem>> GetAllVideosAsync();
    Task<List<VideoItem>> GetVideosByPlaylistAsync(int playlistId);
    Task<VideoItem?> GetVideoByIdAsync(int id);
    Task<int> SaveVideoAsync(VideoItem video);
    Task<int> DeleteVideoAsync(VideoItem video);
    Task<int> ToggleWatchedAsync(VideoItem video);

    // סטטיסטיקות ואגרגציות (Statistics & Aggregations)
    Task<PlaylistStats> GetPlaylistStatsAsync(int playlistId);

    // חיפוש ומיון מתקדם ב-SQL (Filtering & Sorting)
    Task<List<VideoItem>> GetFilteredVideosAsync(
        int playlistId,
        string? searchQuery = null,
        bool onlyUnwatched = false,
        VideoSortOption sortBy = VideoSortOption.NewestFirst);
}
