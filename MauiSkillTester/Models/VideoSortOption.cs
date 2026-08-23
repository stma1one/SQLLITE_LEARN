namespace MauiSkillTester.Models;

/// <summary>
/// אפשרויות מיון עבור רשימת הסרטונים ב-SQL
/// </summary>
public enum VideoSortOption
{
    NewestFirst,
    ShortestFirst,
    LongestFirst,
    Alphabetical
}

/// <summary>
/// מודל עזר לתצוגת אפשרויות המיון ב-Picker עם כותרות ידידותיות בעברית
/// </summary>
public class VideoSortOptionItem
{
    public VideoSortOption Option { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public static List<VideoSortOptionItem> AllOptions => new()
    {
        new VideoSortOptionItem { Option = VideoSortOption.NewestFirst, DisplayName = "🆕 החדשים ביותר" },
        new VideoSortOptionItem { Option = VideoSortOption.ShortestFirst, DisplayName = "⏱️ הקצרים ביותר" },
        new VideoSortOptionItem { Option = VideoSortOption.LongestFirst, DisplayName = "⏳ הארוכים ביותר" },
        new VideoSortOptionItem { Option = VideoSortOption.Alphabetical, DisplayName = "🔤 לפי שם (א-ב)" }
    };
}
