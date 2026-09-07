# מדריך יצירת הדמיות HTML וצילום מסך עבור מדריכי MAUI

מסמך זה מגדיר את אופן יצירת קובצי ההדמיה (`Learning/Mockups/<ScreenName>.html`) וצילומם באמצעות סוכן הדפדפן (Browser Subagent / MCP) לקבלת צילומי מסך מקצועיים הממחישים את "אסטרטגיית הבלוקים" (The Block Strategy) של Leomaris Reyes.

---

## 📱 1. מטרת ההדמיה ב-HTML/CSS
- במדריכים של Leomaris Reyes, כל מדריך מתחיל בתמונה מרהיבה שמחלקת את המסך לבלוקים צבעוניים (למשל `01_visual_structure.png`).
- מכיוון שאנו בונים מדריכים עשירים ללא תלות באמולטור פעיל, אנו מייצרים קובץ HTML/CSS מדויק שמדמה את ממשק ה-MAUI ביחס של מסך מובייל מודרני (390x844 פיקסלים).
- סוכן הדפדפן פותח את קובץ ה-HTML המקומי ומבצע צילום מסך (`take_screenshot`) הנשמר ישירות בתיקיית `Learning/Images/`.

---

## 🎨 2. תבנית בסיס של Mockup מובייל ב-HTML/CSS

להלן שלד ה-HTML הסטנדרטי שיש ליצור בנתיב:
`Learning/Mockups/<ScreenName>.html`

```html
<!DOCTYPE html>
<html lang="he" dir="rtl">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>MAUI UI Mockup</title>
  <style>
    :root {
      --primary-color: #6366f1;
      --primary-gradient: linear-gradient(135deg, #6366f1 0%, #a855f7 100%);
      --bg-color: #f8fafc;
      --card-bg: #ffffff;
      --text-main: #0f172a;
      --text-muted: #64748b;
      --border-radius: 24px;
      --shadow-soft: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1);
    }

    * {
      box-sizing: border-box;
      margin: 0;
      padding: 0;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
    }

    body {
      background-color: #0f172a;
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 20px;
    }

    /* מסגרת המכשיר הנייד (Device Frame) */
    .phone-frame {
      width: 390px;
      height: 844px;
      background-color: var(--bg-color);
      border-radius: 44px;
      overflow: hidden;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5), 0 0 0 12px #1e293b;
      position: relative;
      display: flex;
      flex-direction: column;
    }

    /* סרגל עליון (Status Bar) */
    .status-bar {
      height: 44px;
      padding: 0 24px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 14px;
      font-weight: 600;
      color: #ffffff;
      z-index: 10;
      background: transparent;
    }

    /* בלוקים צבעוניים להמחשת אסטרטגיית הבלוקים */
    .block-highlight {
      position: relative;
      border: 2px dashed rgba(255, 255, 255, 0.8);
      border-radius: 12px;
      margin: 4px;
    }

    .block-badge {
      position: absolute;
      top: 8px;
      right: 8px;
      background: rgba(15, 23, 42, 0.85);
      color: #fff;
      font-size: 11px;
      font-weight: bold;
      padding: 3px 8px;
      border-radius: 12px;
      z-index: 20;
    }

    /* Hero Section */
    .hero-section {
      height: 240px;
      background: var(--primary-gradient);
      padding: 24px;
      display: flex;
      flex-direction: column;
      justify-content: center;
      color: #ffffff;
      position: relative;
    }

    /* Floating Card */
    .floating-card {
      background: var(--card-bg);
      border-radius: var(--border-radius);
      margin: -50px 20px 16px 20px;
      padding: 24px;
      box-shadow: var(--shadow-soft);
      z-index: 5;
    }

    /* Content Area */
    .content-area {
      flex: 1;
      padding: 0 20px;
      overflow-y: auto;
    }

    .item-card {
      background: #ffffff;
      border-radius: 16px;
      padding: 16px;
      margin-bottom: 12px;
      display: flex;
      align-items: center;
      gap: 16px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04);
    }

    /* Action Bar */
    .action-bar {
      padding: 16px 20px 32px 20px;
      background: #ffffff;
      border-top: 1px solid #e2e8f0;
    }

    .btn-primary {
      width: 100%;
      height: 52px;
      background: var(--primary-gradient);
      color: white;
      border: none;
      border-radius: 16px;
      font-size: 16px;
      font-weight: bold;
      box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
      cursor: pointer;
    }
  </style>
</head>
<body>

  <div class="phone-frame">
    <!-- Status Bar -->
    <div class="status-bar">
      <span>09:41</span>
      <span>📶 🔋</span>
    </div>

    <!-- בלוק 1: Hero -->
    <div class="hero-section block-highlight">
      <span class="block-badge">בלוק 1: Hero Header</span>
      <h2>מוזיקה ופודקאסטים</h2>
      <p style="opacity: 0.9; font-size: 13px;">פלייליסט יומי מותאם אישית</p>
    </div>

    <!-- בלוק 2: Floating Card -->
    <div class="floating-card block-highlight">
      <span class="block-badge">בלוק 2: כרטיס צף (Negative Margin)</span>
      <h3 style="color: var(--text-main); margin-bottom: 4px;">להיטים ישראליים</h3>
      <p style="color: var(--text-muted); font-size: 14px;">24 שירים • עודכן היום</p>
    </div>

    <!-- בלוק 3: Content List -->
    <div class="content-area block-highlight">
      <span class="block-badge">בלוק 3: רשימת תוכן (CollectionView)</span>
      <div class="item-card">
        <div style="width: 44px; height: 44px; background: #e0e7ff; border-radius: 12px; display:flex; align-items:center; justify-content:center;">🎵</div>
        <div style="flex:1;">
          <div style="font-weight: 600; font-size: 15px;">שיר ראשון</div>
          <div style="color: var(--text-muted); font-size: 13px;">אמן מוביל</div>
        </div>
        <span style="color: var(--text-muted); font-size: 13px;">3:42</span>
      </div>
      <div class="item-card">
        <div style="width: 44px; height: 44px; background: #ede9fe; border-radius: 12px; display:flex; align-items:center; justify-content:center;">🎶</div>
        <div style="flex:1;">
          <div style="font-weight: 600; font-size: 15px;">שיר שני</div>
          <div style="color: var(--text-muted); font-size: 13px;">זמרת אורחת</div>
        </div>
        <span style="color: var(--text-muted); font-size: 13px;">4:15</span>
      </div>
    </div>

    <!-- בלוק 4: Action Bar -->
    <div class="action-bar block-highlight">
      <span class="block-badge">בלוק 4: כפתור פעולה (Command)</span>
      <button class="btn-primary">נגן פלייליסט</button>
    </div>
  </div>

</body>
</html>
```

---

## 📸 3. פרוטוקול צילום המסך (כל הבלוקים בנפרד + תמונת שלד)

כאשר מפיקים מדריך:
1. שמור את קובץ ה-HTML בנתיב `Learning/Mockups/<ScreenName>.html`. ודא שכל בלוק מסומן בקלאס/מזהה ברור (`.hero-banner`, `.block-1`, `.block-2` וכו').
2. ודא שתיקיית `Learning/Images/` קיימת.
3. הפעל את סקריפט החילוץ האוטומטי (Playwright / Chrome):
   ```powershell
   python .\Scripts\Capture-Mockup-Blocks.py
   ```
   או השתמש בסוכן הדפדפן (`chrome-devtools-mcp`) לצילום מדויק:
   - צילום מסגרת המכשיר המלאה: `Learning/Images/<ScreenName>-overview.png`.
   - צילום כותרת ה-Hero: `Learning/Images/<ScreenName>-hero.png`.
   - צילום כל בלוק בנפרד: `Learning/Images/<ScreenName>-block<N>-<name>.png`.
4. שלב את תמונת ה-Overview בראש המדריך (חלק 1).
5. **חובה פדגוגית**: שלב את תמונת הבלוק הממוקדת בכל אחד משלבי ה-XAML (חלק 4) ישירות לצד קוד ה-XAML. אין להשאיר שלב ללא המחשה חזותית!
