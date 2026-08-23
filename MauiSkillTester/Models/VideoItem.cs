using SQLite;

namespace MauiSkillTester.Models;

/// <summary>
/// מייצג סרטון וידאו בודד המקושר לפלייליסט (קשר 1-לרבים)
/// </summary>
[Table("Videos")]
public class VideoItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// מפתח זר המצביע על ה-Id של הפלייליסט.
    /// האינדקס מאפשר שליפה סופר-מהירה של כל הסרטונים השייכים לפלייליסט.
    /// </summary>
    [Indexed, NotNull]
    public int PlaylistId { get; set; }

    [Indexed, NotNull]
    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string ChannelName { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    /// <summary>
    /// אינדקס נוסף לסינון מהיר של סרטונים שנצפו לעומת סרטונים שלא נצפו
    /// </summary>
    [Indexed]
    public bool IsWatched { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// תכונת עזר לתצוגת זמן מעוצבת ב-XAML.
    /// האנוטציה [Ignore] מונעת מ-SQLite לנסות ליצור עבורה עמודה בטבלת ה-SQL.
    /// </summary>
    [Ignore]
    public string FormattedDuration => DurationMinutes > 0 ? $"{DurationMinutes} דק'" : "לא צוין";

    /// <summary>
    /// תגית טקסט עבור מצב צפייה
    /// </summary>
    [Ignore]
    public string WatchedStatusText => IsWatched ? "נצפה ✓" : "טרם נצפה ⏳";
}
