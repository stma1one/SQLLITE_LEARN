# MAUI Learning Book — מדריך הלמידה לפיתוח .NET MAUI

ספר זה הוא יומן למידה מצטבר המלווה את הפרויקט, ומסביר כל שלב ומושג בשפה ברורה בעברית עם מונחי קוד מדויקים באנגלית.

---

# שיעור 1 — התקנת SQLite והגדרת מודלים עם מפתחות, אינדקסים וקשרים

_תאריך: 2026-08-19_

## 1. מהו SQLite ולמה משתמשים בו ב-.NET MAUI?
- **SQLite** הוא בסיס נתונים רלציוני (טבלאי) קל-משקל, מהיר ועצמאי לחלוטין (Embedded / Serverless).
- בתוך אפליקציית מובייל ודסקטופ, מסד הנתונים נשמר כקובץ מקומי יחיד (למשל `YouTubeHub.db3`) בתיקיית האחסון המוגנת של האפליקציה (`FileSystem.AppDataDirectory`).
- הנתונים נשמרים לצמיתות (Persistence) גם לאחר סגירת האפליקציה או הפעלה מחדש של המכשיר.

## 2. התקנת חבילות ה-NuGet הנדרשות
כדי לעבוד עם SQLite ב-.NET MAUI בצורה אסינכרונית, אנו מתקינים שתי חבילות עיקריות ב-`MauiSkillTester.csproj`:
1. **`sqlite-net-pcl` (גרסה 1.9.172 ומעלה):** שכבת ה-ORM (Object-Relational Mapping) המאפשרת לנו לעבוד עם אובייקטים ב-C# במקום לכתוב פקודות SQL גולמיות.
2. **`SQLitePCLRaw.bundle_green` (גרסה 2.1.10 ומעלה):** ספריית ה-C הגולמית המבצעת את פעולות הקריאה/כתיבה למנוע ה-SQLite בכל מערכות ההפעלה (Android, iOS, Windows, macOS).

```xml
<ItemGroup>
    <PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
    <PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.10" />
</ItemGroup>
```

## 3. תכונות (Attributes) במודלים של SQLite
כאשר אנחנו מגדירים מחלקה ב-C#, אנחנו מקשטים אותה ואת תכונותיה ב-Attributes מתוך מרחב השמות `using SQLite;`:

| תכונה (Attribute) | משמעות ותפקיד | דוגמה בקוד |
| :--- | :--- | :--- |
| **`[Table("Name")]`** | קובע את השם המדויק של הטבלה במסד הנתונים | `[Table("Videos")]` |
| **`[PrimaryKey]`** | מגדיר את העמודה כמזהה חד-ערכי וייחודי לכל שורה | `[PrimaryKey]` |
| **`[AutoIncrement]`** | מקצה מספר סידורי עולה אוטומטית (1, 2, 3...) בעת `Insert` | `[AutoIncrement]` |
| **`[Indexed]`** | יוצר עץ חיפוש מהיר המונע סריקה של כל הטבלה (Full Scan) | `[Indexed]` |
| **`[NotNull]`** | מבטיח שהשדה חייב להכיל ערך ולא יכול להיות `null` | `[NotNull]` |
| **`[Ignore]`** | מתעלם מהתכונה ולא יוצר עבורה עמודה בטבלה | `[Ignore]` |

## 4. מידול קשר 1-לרבים (One-to-Many Relationship)
בפרויקט שלנו, **פלייליסט אחד** מכיל **הרבה סרטונים**.
- במודל `Playlist` (טבלת האב): יש מפתח ראשי `Id`.
- במודל `VideoItem` (טבלת הבן): יש שדה `PlaylistId` מקושט ב-`[Indexed]`, המצביע ל-`Id` של הפלייליסט שאליו הוא שייך.

```csharp
// Models/Playlist.cs
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

// Models/VideoItem.cs
[Table("Videos")]
public class VideoItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int PlaylistId { get; set; } // מפתח זר

    [Indexed, NotNull]
    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    [Indexed]
    public bool IsWatched { get; set; }

    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
```

## 5. כיצד לוודא את התקינות
1. הרצת `dotnet build` לוודא שהחבילות יורדות בהצלחה והקוד מתקמפל ללא שגיאות.

## 6. שאלות חזרה ובקרה
1. מה ההבדל בין חיפוש שדה שיש עליו `[Indexed]` לבין שדה רגיל ללא אינדקס?
2. למה כדאי להשתמש ב-`sqlite-net-pcl` במקום לכתוב פקודות `CREATE TABLE` גולמיות במחרוזות SQL?

---

# שיעור 2 — שירות בסיס הנתונים האסינכרוני (Database Service), תבנית אתחול עצל ו-Dependency Injection

_תאריך: 2026-08-19_

## 1. ארכיטקטורת שכבת השירות (Service Layer)
במקום לפתוח חיבור למסד הנתונים ישירות בתוך מסכי ה-UI או ה-ViewModels, אנו מרכזים את כל הגישה לנתונים בתוך מחלקת שירות ייעודית:
- **`IDatabaseService`**: ממשק (Interface) המגדיר מה השירות יודע לעשות (CRUD, סינון, מחיקה).
- **`SQLiteDatabaseService`**: המחלקה המממשת את הגישה בפועל באמצעות מנוע SQLite.

הפרדה זו נקראת **הפרדת תחומי אחריות (Separation of Concerns)** והיא מאפשרת לנו:
- להחליף מימוש בעתיד בקלות.
- לכתוב בדיקות יחידה (Unit Tests) עם שירות מדומה (Mock).

## 2. אתחול עצל (Lazy Initialization Pattern)
ב-.NET MAUI, אסור לבצע פעולות I/O או פתיחת חיבורי קבצים בתוך בנאי (`Constructor`).
לכן, אנו משתמשים בתבנית אתחול עצל:

```csharp
public async Task InitializeAsync()
{
    if (_connection != null)
        return; // החיבור כבר אותחל, אין צורך באתחול נוסף

    _connection = new SQLiteAsyncConnection(_databasePath);
    await _connection.CreateTableAsync<Playlist>();
    await _connection.CreateTableAsync<VideoItem>();
    await SeedInitialDataAsync();
}
```
בכל מתודת CRUD, השורה הראשונה היא `await InitializeAsync();`, מה שמבטיח שהטבלאות והחיבור מוכנים בדיוק כשצריך אותם.

## 3. פעולות CRUD אסינכרוניות ושאילתות LINQ

| פעולה | שיטת SQLite | דוגמה בקוד |
| :--- | :--- | :--- |
| **Create (הוספה)** | `InsertAsync` | `await _connection.InsertAsync(newPlaylist);` |
| **Read All (שליפה כוללת)** | `Table<T>().ToListAsync` | `await _connection.Table<Playlist>().OrderByDescending(p => p.CreatedAt).ToListAsync();` |
| **Read by Filter (סינון לפי קשר)** | `Where(...).ToListAsync` | `await _connection.Table<VideoItem>().Where(v => v.PlaylistId == playlistId).ToListAsync();` |
| **Update (עדכון רשומה)** | `UpdateAsync` | `await _connection.UpdateAsync(video);` |
| **Delete (מחיקה)** | `DeleteAsync` | `await _connection.DeleteAsync(video);` |

## 4. מחיקה משורשרת (Cascade Delete)
כאשר מוחקים פלייליסט שלם, עלינו למחוק תחילה את כל הסרטונים השייכים לאותו פלייליסט, כדי שלא יישארו "סרטונים יתומים" במסד הנתונים:

```csharp
public async Task<int> DeletePlaylistAsync(Playlist playlist)
{
    await InitializeAsync();
    
    // 1. מחיקת כל הסרטונים השייכים לפלייליסט
    await _connection.Table<VideoItem>()
                     .Where(v => v.PlaylistId == playlist.Id)
                     .DeleteAsync();

    // 2. מחיקת הפלייליסט עצמו
    return await _connection.DeleteAsync(playlist);
}
```

## 5. רישום ב-Dependency Injection כ-`Singleton`
בתוך `MauiProgram.cs`:
```csharp
builder.Services.AddSingleton<IDatabaseService, SQLiteDatabaseService>();
```
- **`AddSingleton`**: מייצר מופע אחד יחיד שמשותף לכל המסכים וה-ViewModels לאורך כל זמן ריצת האפליקציה. מונע פתיחה כפולה ונעילת קובץ ה-SQLite.

## 6. שאלות חזרה ובקרה
1. למה אסור ליצור את חיבור ה-SQLite בתוך הבנאי (`Constructor`) של ה-Service?
2. מה היתרון בביצוע `Cascade Delete` בעת מחיקת פלייליסט?

---

# שיעור 3 — שכבת ה-ViewModel: ניהול מצב (State), רשימות חכמות (ObservableCollection), פקודות (Commands) ומשגר ה-YouTube (Launcher)

_תאריך: 2026-08-19_

## 1. מבנה שכבת ה-ViewModel
ה-ViewModel הוא המוח של המסך בארכיטקטורת **MVVM** (**M**odel-**V**iew-**V**iew**M**odel):
- **`BaseViewModel`**: מחלקת בסיס המממשת את `INotifyPropertyChanged` ומנהלת את תכונת `IsLoading` (עבור מחוון טעינה `ActivityIndicator`).
- **`MainViewModel`**: ה-ViewModel הראשי המקבל את `IDatabaseService` בבנאי (Constructor Injection), ומנהל את הפלייליסטים, הסרטונים, הסינונים והפקודות.

## 2. מנגנון עדכון המסך: `INotifyPropertyChanged`
כאשר ערך של תכונה משתנה ב-C#, עלינו ליידע את ה-XAML באמצעות קריאה ל-`OnPropertyChanged()`:

```csharp
public Playlist? SelectedPlaylist
{
    get => _selectedPlaylist;
    set
    {
        if (_selectedPlaylist != value)
        {
            _selectedPlaylist = value;
            OnPropertyChanged(); // מעדכן את המסך
            OnPropertyChanged(nameof(HasSelectedPlaylist));
            ((Command)AddVideoCommand).ChangeCanExecute(); // מעדכן האם כפתור ההוספה פעיל
        }
    }
}
```

## 3. `ObservableCollection<T>` — רשימה חיה
- בניגוד ל-`List<T>` רגיל שאינו שולח התראות למסך, **`ObservableCollection<T>`** מודיעה ל-XAML בכל פעם שנוסף פריט (`Add`), נמחק פריט (`Remove`) או מתנקה הרשימה (`Clear`).
- המסך ב-XAML מאזין לאירוע זה ומצייר את הפריטים מחדש באופן אוטומטי.

## 4. פקודות ב-MVVM (`ICommand` ו-`CanExecute`)
במקום לכתוב אירועי `Button_Clicked` בקובץ ה-Code-Behind של המסך, אנו מגדירים פקודות:

```csharp
AddVideoCommand = new Command(
    execute: async () => await AddVideoAsync(),
    canExecute: () => SelectedPlaylist != null &&
                      !string.IsNullOrWhiteSpace(NewVideoTitle) &&
                      !string.IsNullOrWhiteSpace(NewVideoUrl)
);
```
- **`execute`**: הקוד שירוץ בעת לחיצה על הכפתור.
- **`canExecute`**: תנאי לוגי הקובע האם הכפתור יהיה זמין (`Enabled`) או מנוטרל (אפור) ב-UI.

## 5. פתיחת קישור ל-YouTube עם `Launcher`
שירות ה-`Launcher` המובנה ב-.NET MAUI מאפשר לשגר קישורים חיצוניים ישירות למערכת ההפעלה:

```csharp
Uri uri = new Uri(video.Url);
await Launcher.Default.OpenAsync(uri);
```
מערכת ההפעלה (Android, iOS או Windows) תפתח אוטומטית את אפליקציית YouTube הרשמית או את הדפדפן להצגת הסרטון.

## 6. שאלות חזרה ובקרה
1. למה עדיף להשתמש ב-`Command` עם `CanExecute` במקום לכתוב קוד בלחיצת כפתור `Button_Clicked`?
2. כיצד `ObservableCollection` גורמת למסך להתעדכן מיד כשנוסף סרטון חדש?

---

# שיעור 4 — עיצוב ממשק משתמש ב-XAML: תמיכה ב-RTL (עברית), Compiled Bindings, CollectionView ו-ActivityIndicator

_תאריך: 2026-08-19_

## 1. תמיכה בשפות מימין לשמאל (`FlowDirection="RightToLeft"`)
ב-.NET MAUI, תמיכה מלאה בשפות כמו עברית וערבית מתבצעת בקלות ע"י הגדרת תכונת ה-`FlowDirection` בתוך רכיב ה-`ContentPage`:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:MauiSkillTester.ViewModels"
             x:Class="MauiSkillTester.MainPage"
             x:DataType="vm:MainViewModel"
             Title="{Binding Title}"
             FlowDirection="RightToLeft">
```
כאשר `FlowDirection="RightToLeft"`, כל הרכיבים (רשימות, כפתורים, תגיות, שדות קלט) משנים את סידורם אוטומטית מימין לשמאל, והטקסטים מיושרים לימין כנדרש בעברית.

## 2. קישורים מתוכפלים בזמן קומפילציה (`Compiled Bindings / x:DataType`)
כאשר אנחנו מציינים `x:DataType="vm:MainViewModel"` בראש דף ה-XAML:
- המהדר (Compiler) של .NET MAUI בודק בזמן הקומפילציה שכל שם תכונה ב-`Binding` אכן קיים ב-`MainViewModel`.
- אם יש טעות כתיב (למשל `Titlee` במקום `Title`), הקומפילציה תיכשל מיד במקום להיכשל בזמן ריצה (Runtime).
- הביצועים משתפרים משמעותית מכיוון שהמערכת מציבה קוד C# ישיר במקום להשתמש ב-Reflection איטי.

## 3. תצוגת אוספים מתקדמת עם `CollectionView`
הרכיב `CollectionView` הוא הרכיב המומלץ והמתקדם ביותר ב-.NET MAUI להצגת רשימות:
- **`ItemsSource="{Binding Videos}"`**: מקשר את הרשימה ל-`ObservableCollection<VideoItem>` ב-ViewModel.
- **`DataTemplate`**: קובע כיצד ייראה כל פריט ברשימה. אנו מציינים `x:DataType="models:VideoItem"` בתוך התבנית כדי לקבל Compiled Binding עבור שדות הסרטון הבודד.
- **`EmptyView`**: תצוגה מותאמת אישית המוצגת אוטומטית כאשר הרשימה ריקה (`Videos.Count == 0`).

```xml
<CollectionView ItemsSource="{Binding Videos}">
    <CollectionView.EmptyView>
        <VerticalStackLayout Padding="30" HorizontalOptions="Center">
            <Label Text="📭 אין סרטונים להצגה בפלייליסט זה" TextColor="Gray" />
        </VerticalStackLayout>
    </CollectionView.EmptyView>
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:VideoItem">
            <!-- עיצוב כרטיס סרטון -->
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

## 4. מחוון טעינה אסינכרוני (`ActivityIndicator`)
אנו מציגים גלגל טעינה מסתובב בזמן שהנתונים נטענים מ-SQLite:
- **`IsRunning="{Binding IsLoading}"`**: מפעיל את האנימציה כש-`IsLoading == true`.
- **`IsVisible="{Binding IsLoading}"`**: מעלים את הרכיב כליל מהמסך כש-`IsLoading == false`.
- **ארכיטקטורה נקייה:** לעולם אין להשתמש ב-`Page.IsBusy` הישן, אלא לנהל את המצב ב-ViewModel.

## 5. שינוי עיצוב דינמי ב-XAML עם `DataTrigger`
במקום ליצור Convertor ייעודי, XAML מאפשר להגדיר טריגר דינמי המשנה את המראה של כפתור לפי ערך ב-Binding:

```xml
<Button Text="טרם נצפה 👁️"
        BackgroundColor="#FFF3E0"
        TextColor="#E65100">
    <Button.Triggers>
        <DataTrigger TargetType="Button" Binding="{Binding IsWatched}" Value="True">
            <Setter Property="Text" Value="נצפה ✅" />
            <Setter Property="BackgroundColor" Value="#E8F5E9" />
            <Setter Property="TextColor" Value="#2E7D32" />
        </DataTrigger>
    </Button.Triggers>
</Button>
```

## 6. הזרקת תלויות (DI) של דף ה-MainPage
בתוך `MauiProgram.cs` רשמנו את הדף וה-ViewModel:
```csharp
builder.Services.AddSingleton<MainViewModel>();
builder.Services.AddSingleton<MainPage>();
```
ובקובץ הקוד של הדף (`MainPage.xaml.cs`), הבנאי מקבל את ה-ViewModel ומגדיר אותו כ-`BindingContext`:
```csharp
public MainPage(MainViewModel viewModel)
{
    InitializeComponent();
    BindingContext = viewModel;
}

protected override async void OnAppearing()
{
    base.OnAppearing();
    if (BindingContext is MainViewModel viewModel)
    {
        await viewModel.LoadDataAsync();
    }
}
```

## 7. שאלות חזרה ובקרה
1. מה היתרון הבולט של `x:DataType` (Compiled Bindings) ב-XAML?
2. כיצד התכונה `FlowDirection="RightToLeft"` משפיעה על סידור הרכיבים במסך?

---

# שיעור 5 — בדיקות יחידה ב-xUnit, דיאגנוסטיקת קריסות בזמן ריצה ותבנית AAA

_תאריך: 2026-08-19_

## 1. מהן בדיקות יחידה (Unit Testing) ולמה הן חיוניות?
בדיקת יחידה היא קוד אוטומטי הבוחן רכיב לוגי בודד (כגון מתודת בסיס נתונים או פקודה ב-ViewModel) באופן מבודד לחלוטין מיתר חלקי המערכת.
- **גילוי באגים מוקדם:** מונע תקלות קריטיות בזמן ריצה אצל המשתמש.
- **הרצה מהירה במילי-שניות:** מאפשר להריץ עשרות בדיקות בשנייה בודדת עם `dotnet test`.

## 2. תבנית הבדיקה Standard AAA (Arrange, Act, Assert)
כל בדיקה ב-xUnit נכתבת במבנה של 3 שלבים ברורים:

```csharp
[Fact]
public async Task SavePlaylistAsync_ShouldInsertNewPlaylist_AndAssignId()
{
    // 1. Arrange (הכנה) - הגדרת הרכיבים והנתונים
    var service = CreateService();
    var newPlaylist = new Playlist { Name = "סרטוני תכנות" };

    // 2. Act (ביצוע) - הפעלת המתודה הנבדקת
    int result = await service.SavePlaylistAsync(newPlaylist);

    // 3. Assert (אימות) - בדיקת התוצאה
    Assert.True(result > 0);
    Assert.True(newPlaylist.Id > 0);
}
```

## 3. דיאגנוסטיקה ותיקון קריסות בזמן ריצה (Runtime Crash Diagnostics)
במהלך הפיתוח, האפליקציה נתקלה בשתי סיבות מרכזיות לקריסה בזמן הניווט, אותן זיהינו ותיקנו:

1. **`MissingMethodException` ב-Shell Navigation:**
   - **הבעיה:** הרכיב `ShellContent` ב-`AppShell.xaml` ניסה ליצור מופע של `MainPage` דרך `DataTemplate`, שדרש בנאי ללא פרמטרים.
   - **הפתרון:** הוספת בנאי ברירת מחדל ב-`MainPage.xaml.cs`:
   ```csharp
   public MainPage() : this(new MainViewModel(new SQLiteDatabaseService()))
   {
   }
   ```

2. **`IndexOutOfRangeException` בעת הצגת הודעות שגיאה:**
   - **הבעיה:** פנייה ל-`Application.Current.Windows[0]` בזמן אתחול לפני שהחלון הראשון נוצר.
   - **הפתרון:** כתיבת עוזר בטוח `ShowAlertAsync` המשתמש ב-`Shell.Current.DisplayAlertAsync` עם בדיקת תקינות.

## 4. גילוי באג מורכב בעזרת בדיקות היחידה (Race Condition / State Bug)
כאשר הרצנו את בדיקות ה-xUnit, בדיקת טעינת הנתונים נכשלה מכיוון שהתכונה `IsLoading = true` חסמה את טעינת הסרטונים בעת עדכון הפלייליסט הנבחר.
בעזרת הבדיקה תיקנו את הלוגיקה ב-`MainViewModel.cs` כך שכל הסרטונים נטענים בצורה אסינכרונית בטוחה!

## 5. שאלות חזרה ובקרה
1. מה התפקיד של כל אחד משלושת השלבים בתבנית AAA?
2. כיצד בדיקות יחידה ב-xUnit עזרו לנו לגלות באגים בקוד עוד לפני שהרצנו את המכשיר הנייד?

---

# פרק 6: תצורת SQLite רשמית של Microsoft, ניקוי התנגשויות ו-Dependency Injection

בפרק זה יישרנו את הפרויקט ב-100% מול התקן והתיעוד הרשמי של מיקרוסופט ([Microsoft Learn: .NET MAUI Local Databases with SQLite](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/database-sqlite)) ופתרנו קריסות מערכת בעת עליית האפליקציה.

## 1. ניקוי התנגשויות ספריות נייטיביות (Native Provider Bundles Conflict)
### שורש הבעיה שגרמה לקריסה:
בקובץ הפרויקט `MauiSkillTester.csproj` הוגדרו מספר חבילות NuGet של ספקי SQLite שהתנגשו זו בזו:
- `SQLitePCLRaw.bundle_green`
- `SQLitePCLRaw.bundle_e_sqlite3`
- `SQLitePCLRaw.lib.e_sqlite3.android`

בזמן עליית האפליקציה, כאשר נקראה המתודה `Batteries_V2.Init()`, המערכת ניסתה לטעון שני ספקים מקבילים עם P/Invoke סותרים, מה שגרם לקריסה מיידית של האפליקציה (Native Crash).

### הפתרון:
הסרת כל החבילות העודפות והשארת התקן המומלץ והנקי של מיקרוסופט בלבד:
- `sqlite-net-pcl` (גרסה 1.11.285 ומעלה)
- `SQLitePCLRaw.bundle_green` (גרסה 2.1.11 ומעלה)

## 2. מחלקת הקבועים המרכזית (`Constants.cs`)
מיקרוסופט ממליצה לרכז את כל נתוני התצורה של מסד הנתונים בקובץ סטטי מרכזי:

```csharp
using SQLite;

namespace MauiSkillTester;

public static class Constants
{
    public const string DatabaseFilename = "YouTubeHub.db3";

    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}
```

### הסבר על הדגלים (`SQLiteOpenFlags`):
1. **`ReadWrite`**: מאפשר גם קריאת נתונים (SELECT) וגם כתיבה/עדכון (INSERT/UPDATE/DELETE).
2. **`Create`**: יוצר את קובץ ה-`.db3` באופן אוטומטי אם הוא אינו קיים במכשיר.
3. **`SharedCache`**: מאפשר גישה מרובת תהליכונים (Multi-threaded access) ושיתוף זיכרון מטמון למניעת נעילת קבצים.

## 3. אתחול מודולרי ב-`SQLiteDatabaseService.cs`
עדכנו את בנאי השירות כך שייעשה שימוש ב-`Constants.DatabasePath` וב-`Constants.Flags`, עם תמיכה באתחול עצל (Lazy Initialization) ויכולת העברת נתיב מותאם לצורכי בדיקות יחידה:

```csharp
public SQLiteDatabaseService() : this(Constants.DatabasePath, Constants.Flags)
{
}

public SQLiteDatabaseService(string databasePath, SQLiteOpenFlags flags = Constants.Flags)
{
    _databasePath = databasePath;
    _flags = flags;
}

public async Task InitializeAsync()
{
    if (_connection != null)
        return;

    SQLitePCL.Batteries_V2.Init();
    _connection = new SQLiteAsyncConnection(_databasePath, _flags);

    await _connection.CreateTableAsync<Playlist>();
    await _connection.CreateTableAsync<VideoItem>();
    await SeedInitialDataAsync();
}
```

## 4. רישום שירותים במכולת התלויות ב-`MauiProgram.cs` (Dependency Injection)
כדי להבטיח שכל חלקי האפליקציה חולקים מופע יחיד של שירות הנתונים (Singleton), רשמנו את השירותים, את ה-ViewModel ואת ה-Page ב-`MauiProgram.cs`:

```csharp
// רישום שכבת הנתונים, ה-ViewModel וה-View במכולת התלויות (DI Container)
builder.Services.AddSingleton<IDatabaseService, SQLiteDatabaseService>();
builder.Services.AddSingleton<MainViewModel>();
builder.Services.AddSingleton<MainPage>();
```

## 5. שאלות חזרה ובקרה
1. מדוע התנגשות בין ספריות `SQLitePCLRaw` גורמת לקריסה של האפליקציה?
2. מה היתרון של שימוש ב-`Constants.Flags` הכולל `SharedCache` בעת פתיחת חיבור מסד הנתונים?
3. כיצד שיטת ה-Singleton ב-`MauiProgram.cs` מונעת שגיאות של Database Locked?

---

# פרק 7: שאילתות אגרגציה ב-SQLite, פונקציית COALESCE ואנוטציית [Ignore]

_תאריך: 2026-08-20_

בפרק זה למדנו כיצד להפיק סטטיסטיקות וסיכומי נתונים בעלי ביצועים גבוהים ישירות ממנוע ה-SQLite, להגן מפני ערכי `NULL`, ולהגדיר תכונות עזר לתצוגה ב-XAML מבלי לפגוע במבנה הטבלה.

## 1. למה לבצע אגרגציות ב-SQL ולא ב-C#?
כאשר באפליקציה יש מאות או אלפי רשומות, משיכת כל הרשומות ל-C# וספירתן בזיכרון (`videos.Count()`) גוזלת זיכרון RAM ומשאבי מעבד מיותרים.
שאילתות אגרגציה ב-SQL (`COUNT`, `SUM`, `AVG`) מבוצעות ישירות על קובץ בסיס הנתונים ומחזירות **ערך מספרי בודד** במיקרו-שניות.

## 2. ביצוע שאילתות סקלריות עם `ExecuteScalarAsync<T>`
כאשר שאילתת SQL מחזירה ערך בודד בלבד (כמו מספר `int`), נשתמש במתודה `ExecuteScalarAsync<T>`:

```csharp
public async Task<PlaylistStats> GetPlaylistStatsAsync(int playlistId)
{
    await InitializeAsync();

    // 1. ספירת סך כל הסרטונים בפלייליסט
    int totalVideos = await _connection!.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM Videos WHERE PlaylistId = ?", playlistId);

    // 2. ספירת סרטונים שנצפו
    int watchedVideos = await _connection!.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM Videos WHERE PlaylistId = ? AND IsWatched = 1", playlistId);

    // 3. סכימת סך כל דקות הצפייה
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
```

## 3. פונקציית `COALESCE` להגנה מפני ערכי `NULL`
כאשר מבצעים `SUM(DurationMinutes)` על פלייליסט שאין בו סרטונים כלל, SQL מחזיר ערך `NULL` (ולא `0`).
הפונקציה `COALESCE(ערך, ערך_ברירת_מחדל)` מבטיחה שאם התוצאה היא `NULL`, יוחזר הערך `0` ובכך מונעת חריגות וקריסות המרה ב-C#.

## 4. מודל DTO ייעודי לסטטיסטיקות (`PlaylistStats.cs`)
יצרנו מחלקה קלה (Data Transfer Object) המרכזת את תוצאות השאילתות ומכילה תכונות מחושבות לתצוגה:

```csharp
public class PlaylistStats
{
    public int PlaylistId { get; set; }
    public int TotalVideos { get; set; }
    public int WatchedVideos { get; set; }
    public int UnwatchedVideos => TotalVideos - WatchedVideos;
    public int TotalDurationMinutes { get; set; }

    public double CompletionPercentage =>
        TotalVideos > 0 ? Math.Round((double)WatchedVideos / TotalVideos * 100, 1) : 0.0;

    public string FormattedTotalDuration
    {
        get
        {
            if (TotalDurationMinutes < 60)
                return $"{TotalDurationMinutes} דקות";
            int hours = TotalDurationMinutes / 60;
            int mins = TotalDurationMinutes % 60;
            return mins > 0 ? $"{hours} שעות ו-{mins} דק'" : $"{hours} שעות";
        }
    }
}
```

## 5. אנוטציית `[SQLite.Ignore]` ב-Models
ב-`sqlite-net-pcl`, כל תכונה ציבורית (`public Property`) במחלקה נחשבת כעמודה בטבלה.
כאשר אנו רוצים להוסיף תכונת עזר מחושבת עבור ה-XAML (כמו `FormattedDuration`), אנו מוסיפים את האנוטציה `[Ignore]`:

```csharp
[Ignore]
public string FormattedDuration => DurationMinutes > 0 ? $"{DurationMinutes} דק'" : "לא צוין";

[Ignore]
public string WatchedStatusText => IsWatched ? "נצפה ✓" : "טרם נצפה ⏳";
```
באמצעות `[Ignore]`, מנוע ה-SQLite מתעלם לחלוטין מתכונות אלו בעת יצירת הטבלה או שמירת רשומות.

## 6. תצוגת דשבורד סטטיסטי ב-XAML (חיבור ה-DTO לממשק)

ב-`MainPage.xaml`, הוספנו סרגל כרטיסי מידע מודרני המוצג רק כאשר נבחר פלייליסט עם נתונים (`IsVisible="{Binding HasStats}"`).
הממשק משתמש ב-`Border` עם פינות מעוגלות (`RoundRectangle 8`), התאמה אוטומטית למצב כהה ובהיר (`AppThemeBinding`), וחלוקה לשלוש עמודות שוות:

![כרטיסי סיכום סטטיסטי](Images/MainPage-block2-stats.png)

```xml
<Border StrokeShape="RoundRectangle 10"
        Padding="14"
        BackgroundColor="{AppThemeBinding Light=#EDE7F6, Dark=#311B92}"
        IsVisible="{Binding HasStats}">
    <VerticalStackLayout Spacing="8">
        <Label Text="📊 סיכום סטטיסטי (SQLite Aggregations)"
               FontSize="15"
               FontAttributes="Bold"
               TextColor="{AppThemeBinding Light=#4A148C, Dark=#E1BEE7}" />

        <Grid ColumnDefinitions="*, *, *" ColumnSpacing="8">
            <!-- כרטיס 1: סה"כ סרטונים -->
            <Border StrokeShape="RoundRectangle 8"
                    BackgroundColor="{AppThemeBinding Light=White, Dark=#4527A0}"
                    Padding="8">
                <VerticalStackLayout HorizontalOptions="Center" Spacing="2">
                    <Label Text="🎬 סה&quot;כ"
                           FontSize="11"
                           TextColor="Gray"
                           HorizontalOptions="Center" />
                    <Label Text="{Binding CurrentPlaylistStats.TotalVideos}"
                           FontSize="18"
                           FontAttributes="Bold"
                           HorizontalOptions="Center" />
                </VerticalStackLayout>
            </Border>

            <!-- כרטיס 2: סרטונים שנצפו ואחוז השלמה -->
            <Border StrokeShape="RoundRectangle 8"
                    BackgroundColor="{AppThemeBinding Light=White, Dark=#4527A0}"
                    Padding="8">
                <VerticalStackLayout HorizontalOptions="Center" Spacing="2">
                    <Label Text="✅ נצפו"
                           FontSize="11"
                           TextColor="Gray"
                           HorizontalOptions="Center" />
                    <Label Text="{Binding CurrentPlaylistStats.WatchedVideos, StringFormat='{0} סרטונים'}"
                           FontSize="13"
                           FontAttributes="Bold"
                           HorizontalOptions="Center" />
                    <Label Text="{Binding CurrentPlaylistStats.CompletionPercentage, StringFormat='({0}%)'}"
                           FontSize="11"
                           TextColor="#2E7D32"
                           HorizontalOptions="Center" />
                </VerticalStackLayout>
            </Border>

            <!-- כרטיס 3: סך דקות צפייה -->
            <Border StrokeShape="RoundRectangle 8"
                    BackgroundColor="{AppThemeBinding Light=White, Dark=#4527A0}"
                    Padding="8">
                <VerticalStackLayout HorizontalOptions="Center" Spacing="2">
                    <Label Text="⏱️ סה&quot;כ זמן"
                           FontSize="11"
                           TextColor="Gray"
                           HorizontalOptions="Center" />
                    <Label Text="{Binding CurrentPlaylistStats.FormattedTotalDuration}"
                           FontSize="13"
                           FontAttributes="Bold"
                           HorizontalOptions="Center" />
                </VerticalStackLayout>
            </Border>
        </Grid>
    </VerticalStackLayout>
</Border>
```

## 7. אימות מלא בבדיקות יחידה (xUnit)
כתבנו בדיקות יחידה ייעודיות ב-`DatabaseServiceTests.cs` וב-`MainViewModelTests.cs` שמאמתות:
1. חישוב נכון ומדויק של אחוזי השלמה, סה"כ זמנים וספירת פריטים שנצפו.
2. טיפול תקין בפלייליסט ריק (החזרת 0 ואחוז 0 ללא שגיאות).
3. כל 15 בדיקות היחידה עוברות בהצלחה מלאה (100% PASS)!

## 8. שאלות חזרה, הבנה ואתגר חשיבה

### ⚖️ דילמת תכנון: מתי לבצע אגרגציות ב-SQL ומתי ב-C#?
נניח שבאפליקציה יש 1,000 סרטונים.
- **דרך א':** לשלוף את כולם לזיכרון ה-RAM ב-C# ולחשב `Videos.Count()`.
- **דרך ב':** לשלוח שאילתת `SELECT COUNT(*)` ישירות ל-SQLite.
איזו דרך תצרוך פחות סוללה וזיכרון במכשיר נייד, ומדוע?

### 🧠 שאלות הבנה מעמיקות עם רמזים
1. **מה ההבדל המעשי בין `QueryAsync<T>` לבין `ExecuteScalarAsync<T>`?**
   > [!TIP]
   > **רמז לחשיבה:** חשבו על הערך המוחזר — האם מדובר באוסף של שורות שלמות (`List<T>`) או בערך סקלרי בודד (כמו מספר שלם יחיד)?
2. **מדוע חיוני להשתמש בפונקציה `COALESCE(SUM(...), 0)` כשמחשבים זמן כולל?**
   > [!TIP]
   > **רמז לחשיבה:** מה מחזירה שאילתת `SUM` כאשר הטבלה ריקה לגמרי ואין בה אף שורה?
3. **מה היה קורה למבנה הטבלה במסד הנתונים אילו שכחנו להוסיף `[Ignore]` מעל התכונה `FormattedDuration`?**
   > [!TIP]
   > **רמז לחשיבה:** `sqlite-net-pcl` מנסה להפוך כל `public property` לעמודה ב-SQL. האם ניתן לשמור `string` מחושב בטבלה ללא שדה תואם?

---

# פרק 8: שאילתות פרמטריות דינמיות ב-SQL, מנגנון LIKE עם ESCAPE ומיון חכם

_תאריך: 2026-08-20_

בפרק זה שדרגנו את מנגנון החיפוש והסינון של האפליקציה: במקום למשוך את כל הנתונים לזיכרון ה-RAM ולסנן ב-C# באמצעות LINQ, העברנו את העבודה הכבדה ישירות למנוע ה-SQLite באמצעות **שאילתות פרמטריות דינמיות**.

## 1. למה סינון ב-SQL עדיף על סינון ב-C# (In-Memory)?
- **חיסכון בזיכרון:** כאשר יש אלפי סרטונים, טעינת כולם לזיכרון והפעלת `Where` ב-C# גורמת לתקיעות (Lag) וצריכת סוללה.
- **מהירות גבוהה:** מנוע ה-SQLite משתמש באינדקסים שהגדרנו (`[Indexed]`) ומחזיר רק את השורות הרלוונטיות ביותר.

## 2. בניית שאילתה דינמית עם פרמטריזציה (`?`)
במתודה `GetFilteredVideosAsync` אנו בונים את השאילתה בצורה מודולרית תוך שמירה על עקרון הברזל של אבטחת מידע — **לעולם לא לשרשר קלט משתמש ישירות**:

```csharp
var sqlBuilder = new StringBuilder("SELECT * FROM Videos WHERE PlaylistId = ?");
var parameters = new List<object> { playlistId };

if (onlyUnwatched)
{
    sqlBuilder.Append(" AND IsWatched = 0");
}
```

## 3. מנגנון ה-`LIKE` ותו המילוט `ESCAPE '/'`
כאשר המשתמש מחפש טקסט, אנו רוצים למצוא התאמה גם אם המילה נמצאת באמצע המשפט.
לכן אנו עוטפים את הטקסט בסימני אחוז `$"%{escaped}%"`.

### איך מחפשים סימן אחוז או קו תחתי מילולי מבלי ש-SQL יפרש אותם כפקודות?
משתמשים בהוראת `ESCAPE '/'` ומחליפים תווים מיוחדים לפני שליחתם:

```csharp
if (!string.IsNullOrWhiteSpace(searchQuery))
{
    // מילוט תווים מיוחדים
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
```

## 4. מיון דינמי (`ORDER BY`) ו-`COLLATE NOCASE`
הגדרנו `enum VideoSortOption` המאפשר למשתמש לבחור כיצד למיין את התוצאות:

```csharp
string orderByClause = sortBy switch
{
    VideoSortOption.ShortestFirst => " ORDER BY DurationMinutes ASC, CreatedAt DESC",
    VideoSortOption.LongestFirst => " ORDER BY DurationMinutes DESC, CreatedAt DESC",
    VideoSortOption.Alphabetical => " ORDER BY Title COLLATE NOCASE ASC",
    _ => " ORDER BY CreatedAt DESC"
};

sqlBuilder.Append(orderByClause);
```

> **שים לב:** ההוראה `COLLATE NOCASE` מבטיחה שסרטון שמתחיל באות `a` קטנה יופיע לפני סרטון שמתחיל באות `Z` גדולה, למיון א-ב טבעי ונכון.

## 5. חוויית משתמש בזמן אמת ב-MVVM
ב-`MainViewModel.cs`, הגדרנו שכל שינוי ב-`SearchQuery`, ב-`ShowOnlyUnwatched` או ב-`SelectedSortOptionItem` יפעיל מיד את הטעינה מחדש ממסד הנתונים:

```csharp
public string SearchQuery
{
    get => _searchQuery;
    set
    {
        if (_searchQuery != value)
        {
            _searchQuery = value;
            OnPropertyChanged();
            _ = LoadVideosForSelectedPlaylistAsync();
        }
    }
}
```

## 6. אימות בבדיקות יחידה (xUnit)
כתבנו בדיקות יחידה חדשות המאמתות:
1. סינון לפי ערוץ וכותרת ב-SQL.
2. סינון סרטונים שלא נצפו.
3. מיון עולה ויורד לפי משך הזמן.
4. חיפוש מילולי של `"100%"` בעזרת מנגנון ה-ESCAPE.
5. **כל 19 בדיקות היחידה עברו בהצלחה מלאה (100% PASS)!**

## 7. שאלות חזרה ובקרה
1. מה תפקידה של ההוראה `ESCAPE '/'` בתוך שאילתת `LIKE`?
2. למה השתמשנו ב-`COLLATE NOCASE` במיון לפי שם הסרטון?
3. כיצד השימוש בסימני שאלה `?` מגן על האפליקציה מפני מתקפות SQL Injection?

---

# נספח א' — סדרת מדריכי ממשק משתמש בשיטת הבלוקים המודולרית

חלק זה מרכז את מדריכי הממשק המודולריים של האפליקציה. כל מדריך כולל דיאגרמת בלוקים חזותית, צילומי מסך ממוקדים של כל בלוק, פירוק לגריד ולרכיבי XAML עצמאיים, טיפים מעמיקים לעיצוב פרימיום ב-.NET MAUI, וחיבור מלא ל-MVVM.

### 📚 אינדקס המדריכים:
- [ספריית מדריכי הממשק והעקרונות](Guides/README.md)
- 🎬 **[מדריך פיתוח MainPage — מרכז למידה ומעקב YouTube](Guides/MainPage-Guide.md)**:
  - פירוק מודולרי ל-5 בלוקים חזותיים (ניהול פלייליסט, אגרגציות SQLite, טופס הוספה, סרגל סינון ומיון, ורשימת CollectionView).
  - טכניקות מפתח: `DataTrigger`, `RelativeSource AncestorType`, `AppThemeBinding`, ופקודות `ICommand.CanExecute`.
- *מדריכי מסכים נוספים יתווספו כאן במקביל להרחבת הפרויקט.*

---

# נספח ב' — מקורות השראה, תיעוד וביבליוגרפיה

<div style="font-size: 10pt; color: #475569; line-height: 1.7; background: #f8fafc; padding: 18px 24px; border-radius: 12px; border: 1px solid #e2e8f0;">
ספר למידה זה ומדריכי הממשק שבו פותחו בהשראת מתודולוגיית ההוראה המודולרית <strong>The Block Strategy</strong> של <strong>Leomaris Reyes</strong> (Microsoft MVP, בלוגרית Telerik ויוצרת AskXammy.com), המנגישה עיצובי ממשק מובייל מורכבים ב-.NET MAUI בצורה הדרגתית ומובנית.

<strong>תיעוד רשמי ומקורות להעמקה:</strong>
<ul style="margin-top: 8px; padding-right: 22px;">
  <li><strong>Microsoft Learn (.NET MAUI):</strong> תיעוד רשמי של ארכיטקטורת האפליקציה, מחזור החיים ובקרי הממשק (<a href="https://learn.microsoft.com/dotnet/maui/">learn.microsoft.com/dotnet/maui</a>).</li>
  <li><strong>Data Binding & MVVM:</strong> דפוסי חיבור נתונים, פקודות ICommand ו-Compiled Bindings (<a href="https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/">learn.microsoft.com/dotnet/maui/fundamentals/data-binding</a>).</li>
  <li><strong>SQLite-net-pcl & SQLitePCLRaw:</strong> אחסון מקומי מוצפן, אינדקסים, קשרים טבלאיים ושאילתות מהירות במובייל.</li>
  <li><strong>xUnit Testing Framework:</strong> בדיקות יחידה לתשתיות נתונים ומודלי תצוגה בתבנית Standard AAA.</li>
</ul>
</div>





