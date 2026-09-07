# תבנית מדריך שלב-אחר-שלב: שיטת הבלוקים המודולרית

מדריך זה משמש כתבנית אחידה ליצירת מדריכי ממשק ו-MVVM ב-`.NET MAUI` עבור תלמידים.
שם הקובץ המופק: `Learning/Guides/<ScreenName>-Guide.md`

---

```markdown
# מדריך פיתוח ממשק: <שם המסך / התכונה באנגלית ובעברית>
> שיטת הבלוקים המודולרית לבניית ממשק משתמש ב-.NET MAUI

---

## 🎯 1. מבוא והשראה עיצובית: בונים ממשק פרימיום מאפס
שמתם לב פעם באפליקציות המובילות למסכים עם מראה רענן, כרטיסים צפים ואלמנטים ויזואליים מרשימים? 
במדריך הזה לא רק נתבונן בעיצוב כזה — אלא נלמד לבנות אותו צעד-אחר-צעד, באופן מודולרי ונקי, ישירות ב-XAML וב-C# ב-.NET MAUI!

### מטרת המסך והאתגר העיצובי:
- **מה המסך עושה:** <הסבר תכליתי על תפקיד המסך באפליקציה>
- **האתגר המרכזי:** <למשל: יצירת כרטיס מרחף מעל תמונת רקע, שימוש בגרדיאנט פנימי ללא תמונות סטטיות, ורשימה דינמית עם גלילה חלקה>

### 📱 מפת הבלוקים של המסך (Visual Block Architecture)
![דיאגרמת מבנה הבלוקים של המסך](../Images/<ScreenName>-overview.png)

---

## 🧭 2. איך המדריך הזה בנוי? (The Meta-Frame)
כדי שתוכלו להבין כל שורה ולשלוט בממשק ב-100%, נפעל לפי 3 עקרונות ברורים:
- ➖ **דיאגרמה חזותית מפורקת (Visual Diagram):** המסך מחולק לבלוקים פונקציונליים בצבעים ובשמות מוגדרים.
- ➖ **קוד מודולרי לפי בלוקים (Code by Block):** נתחיל עם שלד הגריד והערות מקום (`<!-- כאן יבוא הבלוק -->`), ונרכיב כל רכיב בנפרד.
- ➖ **התקדמות חזותית צעד-אחר-צעד:** לכל בלוק מוצגת תמונת תוצאה ממוקדת כדי לראות את המסך מתעורר לחיים.

---

## 🔧 3. הכנת סביבת העבודה ומרחבי שמות (Namespaces)
לפני שמתחילים בעיצוב, נוודא שכל התלויות ומרחבי השמות מוגדרים בראש קובץ ה-XAML:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:viewmodels="clr-namespace:<AppNamespace>.ViewModels"
             xmlns:models="clr-namespace:<AppNamespace>.Models"
             x:Class="<AppNamespace>.Views.<ScreenName>Page"
             x:DataType="viewmodels:<ScreenName>ViewModel"
             Title="<כותרת המסך>"
             FlowDirection="RightToLeft">
```

---

## 📐 4. אסטרטגיית הבלוקים (The Block Strategy)

### שלב 4.0: שלד הגריד הראשי ומחוון הטעינה (Main Grid Backbone & Loading)
הגריד הוא עמוד השדרה של המסך. נגדיר שורות ועמודות מדויקות:

```xml
<Grid RowDefinitions="Auto, *">
    <!-- מחוון טעינה (ActivityIndicator) -->
    <ActivityIndicator Grid.Row="0"
                       IsRunning="{Binding IsLoading}"
                       IsVisible="{Binding IsLoading}"
                       Color="{StaticResource Primary}"
                       HeightRequest="40"
                       HorizontalOptions="Center" />

    <!-- מיכל הגלילה הראשי -->
    <ScrollView Grid.Row="1">
        <VerticalStackLayout Padding="16" Spacing="20">
            <!-- כותרת ראשית (Hero Header) -->
            <!-- בלוק 1: ... -->
            <!-- בלוק 2: ... -->
        </VerticalStackLayout>
    </ScrollView>
</Grid>
```

---

### שלב 4.1: כותרת עליונה ובאנר ראשי (Hero Section)

![באנר עליון](../Images/<ScreenName>-hero.png)

> [!IMPORTANT]
> **הנחיית כתיבת קוד (XAML Line-Breaking Standard):** הקפידו לרדת שורה עבור כל תכונה (Attribute-per-line) בכל תגית עם 2 תכונות ומעלה או ביטוי Binding מורכב. הרגל זה מונע גלישת שורות וחיתוך טקסט ב-PDF ומבטיח ספר לימוד קריא ומקצועי.

```xml
<Border StrokeShape="RoundRectangle 12"
        BackgroundColor="#1E88E5"
        Padding="16"
        StrokeThickness="0">
    <VerticalStackLayout Spacing="6">
        <Label Text="<כותרת>"
               FontSize="22"
               FontAttributes="Bold"
               TextColor="White" />
        <Label Text="<תת כותרת>"
               FontSize="14"
               TextColor="#E3F2FD" />
    </VerticalStackLayout>
</Border>
```

---

### שלב 4.2: בלוק 1 — <שם הבלוק>

![תצוגת בלוק 1](../Images/<ScreenName>-block1.png)

```xml
<!-- קוד XAML של בלוק 1 -->
```

---

### שלב 4.3: בלוק 2 — <שם הבלוק>

![תצוגת בלוק 2](../Images/<ScreenName>-block2.png)

```xml
<!-- קוד XAML של בלוק 2 -->
```

---

## ✍️ 5. פינת העמקה והבנה מושגית (Deep-Dive Callouts)

> [!TIP]
> ### 🔍 נקודת מפתח ארכיטקטונית
> <הסבר עמוק ומנומק על רכיב או טכניקה, למשל Border לעומת Frame או שימוש ב-RelativeSource>
>
> 🔗 תיעוד רשמי: [Microsoft Learn - .NET MAUI](https://learn.microsoft.com/dotnet/maui/)

---

## 🔌 6. ארכיטקטורת MVVM וחיבור נתונים (ViewModel & Commands)

כעת נחבר את הממשק למחלקת ה-ViewModel בהתאם לכללי פיתוח מקצועיים:

```csharp
namespace <AppNamespace>.ViewModels;

public class <ScreenName>ViewModel : BaseViewModel
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SaveCommand { get; }

    public <ScreenName>ViewModel(IDatabaseService dbService)
    {
        // עקרון פיתוח מקצועי: הגנה על פקודות באמצעות CanExecute
        SaveCommand = new Command(async () => await OnSaveAsync(), () => !IsLoading);
    }
}
```

---

## ❓ 7. שאלות חזרה, הבנה ואתגר חשיבה

### ⚖️ חלק א': דילמת ארכיטקטורה ועיצוב (Architectural Dilemma)
**דילמה:** <הצגת דילמת תכנון מעשית, למשל: חישוב ב-SQL לעומת חישוב ב-C#>  
**שיקולים לבחינה:**
- שיקול ביצועים וזיכרון.
- שיקול פשטות הקוד וגמישות התצוגה.

### 🧠 חלק ב': שאלות הבנה מעמיקות עם רמזי הכוונה
1. **<שאלה 1>?**
   > [!TIP]
   > **רמז לחשיבה:** <רמז תמציתי ומנחה>

2. **<שאלה 2>?**
   > [!TIP]
   > **רמז לחשיבה:** <רמז תמציתי ומנחה>

### 🛠️ חלק ג': אתגר מעשי קצר לתלמיד (Mini-Challenge)
**המשימה:** <תיאור משימת הרחבה קונקרטית לקוד המסך>  
**צעדי יישום מומלצים:**
1. <שלב 1>
2. <שלב 2>

---

## 📖 נספח: מקורות השראה, תיעוד וביבליוגרפיה
<div style="font-size: 9.5pt; color: #64748b; line-height: 1.6;">
מדריך זה נבנה בהשראת מתודולוגיית הפירוק המודולרי של ממשקי מובייל (The Block Strategy) כפי שפותחה ע"י Leomaris Reyes (AskXammy.com / Microsoft MVP).<br>
תיעוד רשמי ומקורות נוספים:
<ul>
  <li>Microsoft Learn: <a href="https://learn.microsoft.com/dotnet/maui/">מדריך הפיתוח הרשמי ל-.NET MAUI</a></li>
  <li>Microsoft Learn: <a href="https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/">עקרונות Data Binding ו-MVVM</a></li>
</ul>
</div>
```
