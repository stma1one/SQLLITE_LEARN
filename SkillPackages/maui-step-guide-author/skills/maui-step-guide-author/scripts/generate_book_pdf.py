#!/usr/bin/env python3
"""
generate_book_pdf.py
--------------------
A reusable, project-adaptable script for compiling MAUI Learning Books and UI Guides 
into a professional, publication-ready PDF book with full RTL support, embedded high-res 
images, syntax highlighting, and clean code formatting without horizontal scrollbars.

Usage:
    python generate_book_pdf.py [--project-root <path>] [--app-name <name>] [--output-pdf <path>]
"""

import os
import sys
import re
import base64
import argparse
import subprocess
from pathlib import Path

# Ensure UTF-8 output on Windows consoles
if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding="utf-8")
        sys.stderr.reconfigure(encoding="utf-8")
    except Exception:
        pass

def find_chrome_executable() -> str:
    """Finds Google Chrome or Microsoft Edge executable across platforms."""
    candidates = [
        # Windows Chrome
        r"C:\Program Files\Google\Chrome\Application\chrome.exe",
        r"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
        os.path.expandvars(r"%LOCALAPPDATA%\Google\Chrome\Application\chrome.exe"),
        # Windows Edge
        r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        r"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
        os.path.expandvars(r"%LOCALAPPDATA%\Microsoft\Edge\Application\msedge.exe"),
        # macOS
        "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
        "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
        # Linux
        "/usr/bin/google-chrome",
        "/usr/bin/chromium-browser",
        "/usr/bin/chromium",
    ]
    for path in candidates:
        if os.path.exists(path):
            return path
    raise FileNotFoundError("Could not find Google Chrome or Microsoft Edge executable for PDF rendering.")

def find_project_root(explicit_root: str = None) -> Path:
    """Finds project root containing .csproj, .sln, or Learning folder."""
    if explicit_root:
        return Path(explicit_root).resolve()
    
    current = Path.cwd()
    for p in [current, *current.parents]:
        if list(p.glob("*.csproj")) or list(p.glob("*.sln")) or (p / "Learning").exists():
            return p
    return current

def detect_project_info(project_root: Path):
    """Detects MAUI project name, .csproj files, and framework version."""
    csprojs = list(project_root.glob("*/*.csproj")) + list(project_root.glob("*.csproj"))
    # Filter out test projects
    app_projects = [p for p in csprojs if not p.stem.endswith(".Tests") and not p.stem.endswith("Test")]
    
    app_name = app_projects[0].stem if app_projects else project_root.name
    return {
        "app_name": app_name,
        "root": project_root
    }

def highlight_code(code, name, attrs):
    from pygments import highlight
    from pygments.lexers import get_lexer_by_name, TextLexer
    from pygments.formatters import HtmlFormatter

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
    """Converts GitHub-style alerts into attractive callout boxes."""
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

def embed_images(html_text: str, base_dir: Path, learning_dir: Path) -> str:
    """Embeds local images as inline base64 to avoid Chrome file sandbox issues."""
    def replace_img(match):
        src = match.group(1)
        if src.startswith("http://") or src.startswith("https://") or src.startswith("data:"):
            return match.group(0)
        
        clean_src = src.replace("../", "").replace("./", "")
        candidates = [
            base_dir / clean_src,
            learning_dir / clean_src,
            learning_dir / "Images" / Path(clean_src).name,
            base_dir / "Images" / Path(clean_src).name
        ]
        
        found_path = None
        for cand in candidates:
            if cand.exists():
                found_path = cand
                break

        if found_path:
            ext = found_path.suffix.lower().replace(".", "")
            if ext == "jpg": ext = "jpeg"
            try:
                with open(found_path, "rb") as f:
                    b64 = base64.b64encode(f.read()).decode("utf-8")
                return f'<img src="data:image/{ext};base64,{b64}" class="embedded-img" alt="{match.group(2)}"'
            except Exception as e:
                print(f"Warning: Failed embedding image {found_path}: {e}")
        return match.group(0)

    return re.sub(r'<img\s+src="([^"]+)"\s+alt="([^"]*)"', replace_img, html_text)

def generate_html_document(combined_md: str, project_info: dict, learning_dir: Path) -> str:
    from markdown_it import MarkdownIt
    from pygments.formatters import HtmlFormatter

    md = MarkdownIt("commonmark", {"html": True, "highlight": highlight_code}).enable("table")
    html_body = md.render(combined_md)
    html_body = transform_callouts(html_body)
    html_body = embed_images(html_body, learning_dir, learning_dir)

    formatter = HtmlFormatter(style="default")
    pygments_css = formatter.get_style_defs('.highlight')

    app_name = project_info.get("app_name", "MAUI Application")

    return f"""<!DOCTYPE html>
<html lang="he" dir="rtl">
<head>
  <meta charset="UTF-8">
  <title>ספר לימוד והדרכה מעשית — {app_name}</title>
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

    /* New chapter starts on new page */
    h1.chapter-title {{
      page-break-before: always;
      break-before: page;
      margin-top: 10px;
      font-size: 22pt;
      color: #0284c7;
      border-bottom: 3px solid var(--primary);
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

    /* Inline Code */
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

    /* Code Blocks — Print-Perfect Wrapping Without Scrollbars */
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
      overflow-x: hidden !important; /* Eliminates scrollbar on PDF/Print */
      white-space: pre-wrap !important; /* Soft wraps long lines gracefully */
      word-break: break-word !important;
      overflow-wrap: anywhere !important;
      background: #f8fafc !important;
      color: #1e293b;
      font-family: 'Fira Code', Consolas, monospace;
      font-size: 8.5pt; /* Crisp size fitting standard lines */
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
      <p><strong>פרויקט מלווה:</strong> {app_name} (.NET MAUI & SQLite)</p>
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

def main():
    parser = argparse.ArgumentParser(description="Generate MAUI Learning Book PDF.")
    parser.add_argument("--project-root", help="Path to MAUI repository root", default=None)
    parser.add_argument("--app-name", help="App name override", default=None)
    parser.add_argument("--output-pdf", help="Destination PDF path", default=None)
    args = parser.parse_args()

    project_root = find_project_root(args.project_root)
    learning_dir = project_root / "Learning"
    if not learning_dir.exists():
        learning_dir.mkdir(parents=True, exist_ok=True)

    project_info = detect_project_info(project_root)
    if args.app_name:
        project_info["app_name"] = args.app_name

    book_md = learning_dir / "MAUI-Learning-Book.md"
    guides_dir = learning_dir / "Guides"

    print(f"Project root: {project_root}")
    print(f"App name: {project_info['app_name']}")
    
    # Read core book
    if book_md.exists():
        with open(book_md, "r", encoding="utf-8") as f:
            combined_md = f.read()
    else:
        combined_md = f"# יומן למידה — {project_info['app_name']}\n\n"

    # Auto-discover all guides in Learning/Guides/ (excluding README index)
    if guides_dir.exists():
        guide_files = [p for p in sorted(list(guides_dir.glob("*.md"))) if p.name.lower() != "readme.md"]
        for guide_path in guide_files:
            print(f"Discovered guide: {guide_path.name}")
            with open(guide_path, "r", encoding="utf-8") as f:
                guide_content = f.read()
            
            guide_title = guide_path.stem
            # Clean title for heading
            clean_title = guide_title.replace("-Guide", "").replace("_", " ")
            combined_md += f"\n\n---\n\n# פרק הרחבה — מדריך פיתוח מפורט: {clean_title} (שיטת הבלוקים המודולרית)\n\n"
            # Strip initial H1 to preserve hierarchy
            guide_body = re.sub(r'^#\s+.*?\n', '', guide_content, count=1)
            combined_md += guide_body

    output_html = learning_dir / "MAUI-Learning-Book.html"
    output_pdf = Path(args.output_pdf) if args.output_pdf else (learning_dir / "MAUI-Learning-Book.pdf")

    print(f"Rendering HTML document...")
    full_html = generate_html_document(combined_md, project_info, learning_dir)
    with open(output_html, "w", encoding="utf-8") as f:
        f.write(full_html)
    print(f"Wrote HTML to: {output_html}")

    chrome_bin = find_chrome_executable()
    print(f"Using browser executable: {chrome_bin}")
    print(f"Generating PDF via headless print...")

    cmd = [
        chrome_bin,
        "--headless=new",
        "--disable-gpu",
        "--no-pdf-header-footer",
        f"--print-to-pdf={output_pdf}",
        str(output_html)
    ]

    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"Error generating PDF: {result.stderr}")
        sys.exit(result.returncode)

    if output_pdf.exists():
        size_kb = output_pdf.stat().st_size / 1024
        print(f"SUCCESS: PDF generated successfully! ({size_kb:.1f} KB)")
        print(f"PDF Location: {output_pdf}")
    else:
        print("Error: PDF file was not created.")
        sys.exit(1)

if __name__ == "__main__":
    main()
