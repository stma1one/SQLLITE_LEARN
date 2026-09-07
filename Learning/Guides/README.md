# ספריית מדריכי ממשק משתמש בשיטת הבלוקים (UI Step Guides)

תיקייה זו מרכזת מדריכי פיתוח מפורטים שלב-אחר-שלב עבור מסכי האפליקציה, בהשראת מתודולוגיית **"אסטרטגיית הבלוקים" (The Block Strategy)** של **Leomaris Reyes** (AskXammy / Telerik).

---

## 🎯 עקרונות המדריכים לתלמידים
1. **דיאגרמה חזותית מפורקת (Visual Breakdown):** כל מסך מתחיל בהדמיית מובייל המחלקת את המסך לאזורים מוגדרים (Hero, Floating Card, Content List, Actions).
2. **שלד גריד ראשי (Grid Backbone):** הגדרה מדויקת של שורות ועמודות (`RowDefinitions`, `ColumnDefinitions`) עם הערות עוגן (`<!-- בלוק 1 -->`).
3. **בנייה מודולרית של בלוקים (Step-by-Step Blocks):** הרכבת כל בלוק בקוד XAML נקי מבלי להציף את התלמיד בקבצים מונוליתיים.
4. **פינות העמקה (Deep-Dive Callouts):** הסבר עמוק על טכניקות מתקדמות (כמו Margin שלילי לכרטיסים צפים, LinearGradientBrush, ועיגול פינות עם Border).
5. **חיבור נקי ל-MVVM:** קישור נתונים ל-ViewModel, שימוש ב-`ICommand.CanExecute`, וניהול מצבי טעינה ללא `Page.IsBusy`.
6. **שאלות חזרה ובקרה:** שאלות רפלקציה לוודא שהתלמיד מסוגל להסביר כל שורת קוד בעיצוב שבנה.

---

## 📂 נתיבי קבצים נלווים
- מדריכי Markdown: `Learning/Guides/<ScreenName>-Guide.md`
- הדמיות HTML למובייל: `Learning/Mockups/<ScreenName>.html`
- צילומי מסך ותרשימים ויזואליים: `Learning/Images/<ScreenName>-overview.png`

---

## 📚 מדריכי מסכים זמינים
- 🎬 **[מדריך פיתוח MainPage — מרכז למידה ומעקב YouTube](MainPage-Guide.md)**:
  - פירוק ל-5 בלוקים חזותיים (בחירת פלייליסט, אגרגציות נתונים ב-SQLite, טופס הוספה, סרגל סינון ומיון, ורשימת CollectionView).
  - טכניקות מתקדמות: `DataTrigger`, `RelativeSource AncestorType`, `AppThemeBinding`, ו-`CanExecute`.
