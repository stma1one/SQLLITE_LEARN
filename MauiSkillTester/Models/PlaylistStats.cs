namespace MauiSkillTester.Models;

/// <summary>
/// מודל נתונים (DTO) המרכז סטטיסטיקות מחושבות עבור פלייליסט מסוים
/// הנתונים מחושבים ישירות באמצעות שאילתות אגרגציה ב-SQLite
/// </summary>
public class PlaylistStats
{
    public int PlaylistId { get; set; }
    public int TotalVideos { get; set; }
    public int WatchedVideos { get; set; }
    public int UnwatchedVideos => TotalVideos - WatchedVideos;
    public int TotalDurationMinutes { get; set; }

    /// <summary>
    /// אחוז הסרטונים שנצפו מתוך סך הסרטונים בפלייליסט
    /// </summary>
    public double CompletionPercentage =>
        TotalVideos > 0 ? Math.Round((double)WatchedVideos / TotalVideos * 100, 1) : 0.0;

    /// <summary>
    /// תצוגה מעוצבת של זמן הצפייה הכולל בשעות ודקות
    /// </summary>
    public string FormattedTotalDuration
    {
        get
        {
            if (TotalDurationMinutes < 60)
            {
                return $"{TotalDurationMinutes} דקות";
            }
            int hours = TotalDurationMinutes / 60;
            int mins = TotalDurationMinutes % 60;
            return mins > 0 ? $"{hours} שעות ו-{mins} דק'" : $"{hours} שעות";
        }
    }
}
