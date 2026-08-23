using SQLite;

namespace MauiSkillTester.Models;

/// <summary>
/// מייצג פלייליסט / קטגוריה של סרטונים במסד הנתונים SQLite
/// </summary>
[Table("Playlists")]
public class Playlist
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
