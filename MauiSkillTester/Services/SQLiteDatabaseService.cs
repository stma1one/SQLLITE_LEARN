using SQLite;
using MauiSkillTester.Models;

namespace MauiSkillTester.Services;

/// <summary>
/// מימוש שירות בסיס הנתונים SQLite באמצעות SQLiteAsyncConnection
/// </summary>
public class SQLiteDatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _databasePath;
    private readonly SQLiteOpenFlags _flags;

    /// <summary>
    /// בנאי ברירת מחדל המשתמש בקבועי התצורה הרשמיים
    /// </summary>
    public SQLiteDatabaseService() : this(Constants.DatabasePath, Constants.Flags)
    {
    }

    /// <summary>
    /// בנאי המאפשר הזרקת נתיב ודגלים מותאמים (למשל עבור בדיקות יחידה)
    /// </summary>
    public SQLiteDatabaseService(string databasePath, SQLiteOpenFlags flags = Constants.Flags)
    {
        _databasePath = databasePath;
        _flags = flags;
    }

    /// <summary>
    /// אתחול עצל (Lazy Initialization) - יצירת החיבור והטבלאות רק בפנייה הראשונה
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_connection != null)
            return;

        // אתחול ספקי SQLite הנייטיביים
        SQLitePCL.Batteries_V2.Init();

        // יצירת החיבור האסינכרוני עם הנתיב והדגלים המוגדרים
        _connection = new SQLiteAsyncConnection(_databasePath, _flags);

        // יצירת טבלאות לפי המודלים
        await _connection.CreateTableAsync<Playlist>();
        await _connection.CreateTableAsync<VideoItem>();

        // זריעת נתוני דוגמה ראשוניים אם בסיס הנתונים ריק
        await SeedInitialDataAsync();
    }

    private async Task SeedInitialDataAsync()
    {
        if (_connection == null) return;

        var existingPlaylists = await _connection.Table<Playlist>().CountAsync();
        if (existingPlaylists == 0)
        {
            var playlist1 = new Playlist
            {
                Name = "מדריכי .NET MAUI",
                Description = "סרטונים וקורסים לפיתוח אפליקציות מובייל ודסקטופ",
                CreatedAt = DateTime.Now
            };

            var playlist2 = new Playlist
            {
                Name = "מוזיקה לריכוז ולתכנות",
                Description = "פלייליסט של מוזיקת Lo-Fi לפיתוח",
                CreatedAt = DateTime.Now
            };

            await _connection.InsertAsync(playlist1);
            await _connection.InsertAsync(playlist2);

            var video1 = new VideoItem
            {
                PlaylistId = playlist1.Id,
                Title = "מבוא ל-.NET MAUI ב-10 דקות",
                Url = "https://www.youtube.com/watch?v=k_n7qM905e4",
                ChannelName = "dotnet",
                DurationMinutes = 12,
                IsWatched = true,
                Notes = "סרטון פתיחה מעולה להבנת מבנה הפרויקט ו-XAML",
                CreatedAt = DateTime.Now
            };

            var video2 = new VideoItem
            {
                PlaylistId = playlist1.Id,
                Title = "שימוש ב-SQLite ו-MVVM ב-MAUI",
                Url = "https://www.youtube.com/watch?v=F_fKzS2U6Lw",
                ChannelName = "Gerald Versluis",
                DurationMinutes = 18,
                IsWatched = false,
                Notes = "מדריך שלב אחר שלב לעבודה עם בסיסי נתונים",
                CreatedAt = DateTime.Now
            };

            var video3 = new VideoItem
            {
                PlaylistId = playlist2.Id,
                Title = "Lo-Fi Beats to Code/Relax to",
                Url = "https://www.youtube.com/watch?v=jfKfPfyJRdk",
                ChannelName = "Lofi Girl",
                DurationMinutes = 60,
                IsWatched = false,
                Notes = "מוזיקת רקע מצוינת בזמן כתיבת קוד",
                CreatedAt = DateTime.Now
            };

            await _connection.InsertAsync(video1);
            await _connection.InsertAsync(video2);
            await _connection.InsertAsync(video3);
        }
    }

    #region פעולות פלייליסטים (Playlists CRUD)

    public async Task<List<Playlist>> GetPlaylistsAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<Playlist>()
                                 .OrderByDescending(p => p.CreatedAt)
                                 .ToListAsync();
    }

    public async Task<Playlist?> GetPlaylistByIdAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.Table<Playlist>()
                                 .Where(p => p.Id == id)
                                 .FirstOrDefaultAsync();
    }

    public async Task<int> SavePlaylistAsync(Playlist playlist)
    {
        await InitializeAsync();
        if (playlist.Id != 0)
        {
            // עדכון רשומה קיימת (Update)
            return await _connection!.UpdateAsync(playlist);
        }
        else
        {
            // הוספת רשומה חדשה (Insert)
            return await _connection!.InsertAsync(playlist);
        }
    }

    public async Task<int> DeletePlaylistAsync(Playlist playlist)
    {
        await InitializeAsync();
        // מחיקת כל הסרטונים השייכים לפלייליסט (Cascade Delete)
        await _connection!.Table<VideoItem>()
                          .Where(v => v.PlaylistId == playlist.Id)
                          .DeleteAsync();

        // מחיקת הפלייליסט עצמו
        return await _connection!.DeleteAsync(playlist);
    }

    #endregion

    #region פעולות סרטונים (Videos CRUD)

    public async Task<List<VideoItem>> GetAllVideosAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<VideoItem>()
                                 .OrderByDescending(v => v.CreatedAt)
                                 .ToListAsync();
    }

    public async Task<List<VideoItem>> GetVideosByPlaylistAsync(int playlistId)
    {
        await InitializeAsync();
        // שימוש באינדקס על PlaylistId לשליפה מהירה
        return await _connection!.Table<VideoItem>()
                                 .Where(v => v.PlaylistId == playlistId)
                                 .OrderByDescending(v => v.CreatedAt)
                                 .ToListAsync();
    }

    public async Task<VideoItem?> GetVideoByIdAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.Table<VideoItem>()
                                 .Where(v => v.Id == id)
                                 .FirstOrDefaultAsync();
    }

    public async Task<int> SaveVideoAsync(VideoItem video)
    {
        await InitializeAsync();
        if (video.Id != 0)
        {
            // עדכון רשומה קיימת (Update)
            return await _connection!.UpdateAsync(video);
        }
        else
        {
            // הוספת רשומה חדשה (Insert)
            return await _connection!.InsertAsync(video);
        }
    }

    public async Task<int> DeleteVideoAsync(VideoItem video)
    {
        await InitializeAsync();
        return await _connection!.DeleteAsync(video);
    }

    public async Task<int> ToggleWatchedAsync(VideoItem video)
    {
        await InitializeAsync();
        video.IsWatched = !video.IsWatched;
        return await _connection!.UpdateAsync(video);
    }

    #endregion

    #region שאילתות אגרגציה וסטטיסטיקה (SQL Aggregations)

    public async Task<PlaylistStats> GetPlaylistStatsAsync(int playlistId)
    {
        await InitializeAsync();

        // 1. ספירת סך כל הסרטונים בפלייליסט
        int totalVideos = await _connection!.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Videos WHERE PlaylistId = ?", playlistId);

        // 2. ספירת סרטונים שנצפו
        int watchedVideos = await _connection!.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Videos WHERE PlaylistId = ? AND IsWatched = 1", playlistId);

        // 3. סכימת סך כל דקות הצפייה תוך הגנה מפני NULL בעזרת COALESCE
        int totalDuration = await _connection!.ExecuteScalarAsync<int>(
            "SELECT COALESCE(SUM(DurationMinutes), 0) FROM Videos WHERE PlaylistId = ?", playlistId);

        return new PlaylistStats
        {
            PlaylistId = playlistId,
            TotalVideos = totalVideos,
            WatchedVideos = watchedVideos,
            TotalDurationMinutes = totalDuration
        };
    }

    #endregion

    #region חיפוש ומיון מתקדם (Advanced SQL Search & Dynamic Sorting)

    public async Task<List<VideoItem>> GetFilteredVideosAsync(
        int playlistId,
        string? searchQuery = null,
        bool onlyUnwatched = false,
        VideoSortOption sortBy = VideoSortOption.NewestFirst)
    {
        await InitializeAsync();

        var sqlBuilder = new System.Text.StringBuilder("SELECT * FROM Videos WHERE PlaylistId = ?");
        var parameters = new List<object> { playlistId };

        // 1. הוספת תנאי סינון סרטונים שלא נצפו
        if (onlyUnwatched)
        {
            sqlBuilder.Append(" AND IsWatched = 0");
        }

        // 2. הוספת תנאי חיפוש טקסטואלי מרובה-שדות עם מילוט תווים מיוחדים (LIKE ESCAPE)
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            // מילוט תווי Wildcard כדי שייחשבו כתווים מילוליים ולא כפקודות
            string escaped = searchQuery.Trim()
                .Replace("/", "//")
                .Replace("%", "/%")
                .Replace("_", "/_");

            string pattern = $"%{escaped}%";

            sqlBuilder.Append(" AND (Title LIKE ? ESCAPE '/' OR ChannelName LIKE ? ESCAPE '/' OR Notes LIKE ? ESCAPE '/')");
            parameters.Add(pattern);
            parameters.Add(pattern);
            parameters.Add(pattern);
        }

        // 3. הוספת תנאי מיון דינמי (Dynamic ORDER BY)
        string orderByClause = sortBy switch
        {
            VideoSortOption.ShortestFirst => " ORDER BY DurationMinutes ASC, CreatedAt DESC",
            VideoSortOption.LongestFirst => " ORDER BY DurationMinutes DESC, CreatedAt DESC",
            VideoSortOption.Alphabetical => " ORDER BY Title COLLATE NOCASE ASC",
            _ => " ORDER BY CreatedAt DESC"
        };

        sqlBuilder.Append(orderByClause);

        return await _connection!.QueryAsync<VideoItem>(sqlBuilder.ToString(), parameters.ToArray());
    }

    #endregion
}
