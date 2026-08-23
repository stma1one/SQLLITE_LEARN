using SQLite;

namespace MauiSkillTester;

/// <summary>
/// קבועי תצורה מרכזיים עבור מסד הנתונים SQLite לפי התקן הרשמי של Microsoft .NET MAUI
/// Reference: https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/database-sqlite
/// </summary>
public static class Constants
{
    /// <summary>
    /// שם קובץ מסד הנתונים המקומי
    /// </summary>
    public const string DatabaseFilename = "YouTubeHub.db3";

    /// <summary>
    /// דגלי פתיחת החיבור למסד הנתונים:
    /// - ReadWrite: הרשאת קריאה וכתיבה
    /// - Create: יצירת הקובץ אוטומטית אם אינו קיים
    /// - SharedCache: תמיכה בריבוי תהליכונים (Multi-threaded access)
    /// </summary>
    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    /// <summary>
    /// הנתיב המלא לקובץ מסד הנתונים בתיקיית האחסון המאובטחת של האפליקציה בכל מערכת הפעלה
    /// </summary>
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}
