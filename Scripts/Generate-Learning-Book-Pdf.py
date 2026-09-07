#!/usr/bin/env python3
"""
Generate-Learning-Book-Pdf.py
-----------------------------
Generates a complete, high-quality PDF of the MAUI Learning Book and all modular UI guides.
Tailored for YouTube Hub with clean print-CSS, RTL support, embedded high-res images,
and wrap-ready code blocks without horizontal scrollbars.
"""

import os
import sys
import re
import base64
import subprocess
from pathlib import Path
from markdown_it import MarkdownIt
from pygments import highlight
from pygments.lexers import get_lexer_by_name, TextLexer
from pygments.formatters import HtmlFormatter

PROJECT_ROOT = Path(r"c:\Users\Owner\source\repos\MauiSkillTester")
LEARNING_DIR = PROJECT_ROOT / "Learning"
BOOK_MD = LEARNING_DIR / "MAUI-Learning-Book.md"
GUIDE_MD = LEARNING_DIR / "Guides" / "MainPage-Guide.md"
OUTPUT_HTML = LEARNING_DIR / "MAUI-Learning-Book.html"
OUTPUT_PDF = LEARNING_DIR / "MAUI-Learning-Book.pdf"
CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"

def highlight_code(code, name, attrs):
    lang = (name or "").strip().lower()
    if not lang:
        lang = "text"
    try:
        lexer = get_lexer_by_name(lang, stripall=True)
    except Exception:
        lexer = TextLexer()
    formatter = HtmlFormatter(nowrap=True, style="friendly")
    highlighted = highlight(code, lexer, formatter)
    return f'<div class="code-block" dir="ltr"><div class="code-header"><span class="code-lang">{lang.upper()}</span></div><pre class="highlight"><code>{highlighted}</code></pre></div>'

def transform_callouts(html_text: str) -> str:
    patterns = {
        r'<blockquote>\s*<p>\s*\[!TIP\]\s*(.*?)</p>': r'<div class="callout callout-tip"><div class="callout-icon">💡</div><div class="callout-content"><p>\1</p>',
        r'<blockquote>\s*<p>\s*\[!NOTE\]\s*(.*?)</p>': r'<div class="callout callout-note"><div class="callout-icon">ℹ️</div><div class="callout-content"><p>\1</p>',
        r'<blockquote>\s*<p>\s*\[!IMPORTANT\]\s*(.*?)</p>': r'<div class="callout callout-important"><div class="callout-icon">⚠️</div><div class="callout-content"><p>\1</p>',
        r'<blockquote>\s*<p>\s*\[!WARNING\]\s*(.*?)</p>': r'<div class="callout callout-warning"><div class="callout-icon">🚨</div><div class="callout-content"><p>\1</p>',
        r'<blockquote>\s*<p>\s*\[!CAUTION\]\s*(.*?)</p>': r'<div class="callout callout-caution"><div class="callout-icon">🛑</div><div class="callout-content"><p>\1</p>',
    }
    for pat, rep in patterns.items():
        html_text = re.sub(pat, rep, html_text, flags=re.DOTALL)
    html_text = html_text.replace('</blockquote>', '</div></div>')
    return html_text

def embed_images(html_text: str, base_dir: Path) -> str:
    def replace_img(match):
        src = match.group(1)
        if src.startswith("http://") or src.startswith("https://") or src.startswith("data:"):
            return match.group(0)
        
        clean_src = src.replace("../", "").replace("./", "")
        img_path = base_dir / clean_src
        if not img_path.exists():
            img_path = LEARNING_DIR / clean_src
        if not img_path.exists():
            img_path = LEARNING_DIR / "Images" / Path(clean_src).name

        if img_path.exists():
            ext = img_path.suffix.lower().replace(".", "")
            if ext == "jpg": ext = "jpeg"
            try:
                with open(img_path, "rb") as f:
                    b64 = base64.b64encode(f.read()).decode("utf-8")
                return f'<img src="data:image/{ext};base64,{b64}" class="embedded-img" alt="{match.group(2)}"'
            except Exception as e:
                print(f"Error embedding image {img_path}: {e}")
        return match.group(0)

    return re.sub(r'<img\s+src="([^"]+)"\s+alt="([^"]*)"', replace_img, html_text)

def main():
    print("Reading markdown sources...")
    with open(BOOK_MD, "r", encoding="utf-8") as f:
        book_content = f.read()

    guide_content = ""
    if GUIDE_MD.exists():
        with open(GUIDE_MD, "r", encoding="utf-8") as f:
            guide_content = f.read()

    combined_md = book_content
    if guide_content:
        combined_md += "\n\n---\n\n# פרק הרחבה — מדריך פיתוח מפורט: MainPage (שיטת הבלוקים המודולרית)\n\n"
        guide_body = re.sub(r'^#\s+.*?\n', '', guide_content, count=1)
        combined_md += guide_body

    md = MarkdownIt("commonmark", {"html": True, "highlight": highlight_code}).enable("table")
    html_body = md.render(combined_md)
    html_body = transform_callouts(html_body)
    html_body = embed_images(html_body, LEARNING_DIR)

    formatter = HtmlFormatter(style="default")
    pygments_css = formatter.get_style_defs('.highlight')

    full_html = f"""<!DOCTYPE html>
<html lang="he" dir="rtl">
<head>
  <meta charset="UTF-8">
  <title>MAUI Learning Book</title>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Assistant:wght@300;400;600;700;800&family=Fira+Code:wght@400;500;700&display=swap');

    :root {{
      --primary: #0284c7;
      --primary-dark: #0369a1;
      --secondary: #6366f1;
      --accent: #f59e0b;
      --text: #0f172a;
      --text-muted: #475569;
      --bg: #ffffff;
      --bg-alt: #f8fafc;
      --border: #cbd5e1;
      --code-bg: #f8fafc;
      --code-border: #e2e8f0;
    }}

    @page {{
      size: A4;
      margin: 16mm 14mm 18mm 14mm;
      @bottom-left {{
        content: "ספר הלמידה — .NET MAUI K-12";
        font-family: 'Assistant', sans-serif;
        font-size: 8.5pt;
        color: #64748b;
        direction: rtl;
      }}
      @bottom-right {{
        content: counter(page);
        font-family: 'Assistant', sans-serif;
        font-size: 8.5pt;
        color: #64748b;
        font-weight: bold;
      }}
    }}

    * {{
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }}

    body {{
      font-family: 'Assistant', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      direction: rtl;
      text-align: right;
      color: var(--text);
      background: var(--bg);
      line-height: 1.6;
      font-size: 11pt;
    }}

    /* Cover Page */
    .cover-page {{
      page-break-after: always;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      min-height: 82vh;
      text-align: center;
      padding: 35px 20px;
      border: 3px solid var(--border);
      border-radius: 18px;
      background: linear-gradient(180deg, #f0f9ff 0%, #ffffff 100%);
      margin-top: 15px;
    }}

    .cover-badge {{
      background: #e0f2fe;
      color: #0369a1;
      font-size: 12pt;
      font-weight: 700;
      padding: 6px 18px;
      border-radius: 20px;
      margin-bottom: 24px;
      display: inline-block;
      letter-spacing: 0.5px;
    }}

    .cover-title {{
      font-size: 34pt;
      font-weight: 800;
      color: #0c4a6e;
      margin-bottom: 12px;
      letter-spacing: -0.5px;
    }}

    .cover-subtitle {{
      font-size: 14pt;
      color: var(--text-muted);
      margin-bottom: 30px;
      max-width: 540px;
      line-height: 1.5;
    }}

    .cover-divider {{
      width: 70px;
      height: 4px;
      background: var(--primary);
      border-radius: 2px;
      margin: 0 auto 30px auto;
    }}

    .cover-meta {{
      background: #ffffff;
      border: 1px solid var(--border);
      border-radius: 12px;
      padding: 18px 24px;
      max-width: 440px;
      text-align: right;
      font-size: 10pt;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
    }}

    .cover-meta p {{
      margin: 6px 0;
      color: #334155;
    }}

    .cover-meta strong {{
      color: #0f172a;
    }}

    /* Typography & Headings */
    h1 {{
      font-size: 20pt;
      font-weight: 800;
      color: #0369a1;
      margin-top: 28px;
      margin-bottom: 14px;
      padding-bottom: 8px;
      border-bottom: 2px solid #e0f2fe;
      page-break-after: avoid;
      break-after: avoid;
    }}

    h2 {{
      font-size: 14pt;
      font-weight: 700;
      color: #0f172a;
      margin-top: 22px;
      margin-bottom: 10px;
      page-break-after: avoid;
      break-after: avoid;
    }}

    h3 {{
      font-size: 12pt;
      font-weight: 700;
      color: #334155;
      margin-top: 16px;
      margin-bottom: 8px;
      break-after: avoid;
    }}

    p {{
      margin-bottom: 10px;
    }}

    ul, ol {{
      margin-bottom: 12px;
      padding-right: 22px;
    }}

    li {{
      margin-bottom: 5px;
    }}

    /* Code and Inline Code */
    code {{
      font-family: 'Fira Code', Consolas, Monaco, monospace;
      direction: ltr;
      unicode-bidi: isolate;
      font-size: 9pt;
      background: #f1f5f9;
      color: #0969da;
      padding: 2px 6px;
      border-radius: 4px;
      border: 1px solid #cbd5e1;
      vertical-align: middle;
      font-weight: 500;
    }}

    /* Code Blocks — Soft Wrapping & No Scrollbars */
    .code-block {{
      direction: ltr;
      text-align: left;
      margin: 14px 0;
      background: #f8fafc;
      border: 1px solid #cbd5e1;
      border-radius: 8px;
      overflow: hidden;
      break-inside: avoid;
      page-break-inside: avoid;
    }}

    .code-header {{
      background: #e2e8f0;
      padding: 4px 12px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid #cbd5e1;
    }}

    .code-lang {{
      font-size: 8pt;
      font-weight: 700;
      color: #475569;
      font-family: 'Fira Code', monospace;
    }}

    pre.highlight {{
      margin: 0;
      padding: 10px 14px;
      overflow-x: hidden !important; /* Eliminates scrollbar on PDF */
      white-space: pre-wrap !important; /* Soft wraps long lines gracefully */
      word-break: break-word !important;
      overflow-wrap: anywhere !important;
      background: #f8fafc !important;
      color: #1e293b;
      font-family: 'Fira Code', Consolas, monospace;
      font-size: 8.5pt;
      line-height: 1.45;
      tab-size: 2;
    }}

    pre.highlight code {{
      background: transparent !important;
      border: none;
      padding: 0;
      color: inherit;
      font-size: inherit;
      white-space: pre-wrap !important;
      word-break: break-word !important;
      overflow-wrap: anywhere !important;
      unicode-bidi: normal;
    }}

    /* Tables */
    table {{
      width: 100%;
      border-collapse: collapse;
      margin: 16px 0;
      font-size: 10pt;
      break-inside: avoid;
      page-break-inside: avoid;
    }}

    th, td {{
      padding: 8px 12px;
      border: 1px solid var(--border);
      text-align: right;
    }}

    th {{
      background: #f1f5f9;
      font-weight: 700;
      color: #0f172a;
    }}

    tr:nth-child(even) {{
      background: #f8fafc;
    }}

    /* Callouts / Alerts */
    .callout {{
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 12px 16px;
      border-radius: 8px;
      margin: 14px 0;
      border-right: 4px solid;
      background: #f8fafc;
      break-inside: avoid;
    }}

    .callout-icon {{
      font-size: 16pt;
      line-height: 1;
    }}

    .callout-content {{
      flex: 1;
      font-size: 10pt;
    }}

    .callout-content p:last-child {{
      margin-bottom: 0;
    }}

    .callout-tip {{
      border-color: #10b981;
      background: #f0fdf4;
      color: #065f46;
    }}

    .callout-note {{
      border-color: #0284c7;
      background: #f0f9ff;
      color: #0c4a6e;
    }}

    .callout-important {{
      border-color: #f59e0b;
      background: #fffbeb;
      color: #92400e;
    }}

    .callout-warning {{
      border-color: #ef4444;
      background: #fef2f2;
      color: #991b1b;
    }}

    .callout-caution {{
      border-color: #dc2626;
      background: #fef2f2;
      color: #991b1b;
    }}

    /* Images */
    .embedded-img {{
      max-width: 100%;
      height: auto;
      display: block;
      margin: 14px auto;
      border-radius: 8px;
      border: 1px solid var(--border);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
      break-inside: avoid;
      page-break-inside: avoid;
    }}

    hr {{
      border: none;
      border-top: 1px solid var(--border);
      margin: 24px 0;
    }}

    {pygments_css}
  </style>
</head>
<body>

  <!-- Cover Page -->
  <div class="cover-page">
    <div class="cover-badge">ספר לימוד והדרכה מעשית</div>
    <div class="cover-title">MAUI Learning Book</div>
    <div class="cover-subtitle">המדריך המעשי לפיתוח אפליקציות מובייל ודסקטופ ב-.NET MAUI</div>
    <div class="cover-divider"></div>
    <div class="cover-meta">
      <p><strong>פרויקט מלווה:</strong> YouTube Hub (.NET MAUI & SQLite)</p>
      <p><strong>ארכיטקטורה:</strong> MVVM, Service Layer & Repository Pattern</p>
      <p><strong>טכנולוגיות:</strong> C# 12, .NET 10, XAML, SQLite-net-pcl, xUnit</p>
      <p><strong>שנת מהדורה:</strong> 2026</p>
    </div>
  </div>

  <!-- Content Body -->
  <div class="content-container">
    {html_body}
  </div>

</body>
</html>"""

    with open(OUTPUT_HTML, "w", encoding="utf-8") as f:
        f.write(full_html)
    print(f"Writing rendered HTML to: {OUTPUT_HTML}")

    print("Generating PDF using headless Google Chrome...")
    cmd = [
        CHROME_PATH,
        "--headless=new",
        "--disable-gpu",
        "--no-pdf-header-footer",
        f"--print-to-pdf={OUTPUT_PDF}",
        str(OUTPUT_HTML)
    ]

    res = subprocess.run(cmd, capture_output=True, text=True)
    if res.returncode != 0:
        print(f"Chrome PDF Error: {res.stderr}")
        sys.exit(1)

    if OUTPUT_PDF.exists():
        size_kb = OUTPUT_PDF.stat().st_size / 1024
        print(f"SUCCESS: PDF generated successfully! ({size_kb:.1f} KB)")
        print(f"PDF Location: {OUTPUT_PDF}")
    else:
        print("ERROR: PDF file was not created.")
        sys.exit(1)

if __name__ == "__main__":
    main()
