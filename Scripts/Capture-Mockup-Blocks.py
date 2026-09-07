import sys
from pathlib import Path
from playwright.sync_api import sync_playwright

PROJECT_ROOT = Path(r"c:\Users\Owner\source\repos\MauiSkillTester")
MOCKUP_FILE = PROJECT_ROOT / "Learning" / "Mockups" / "MainPage.html"
IMAGES_DIR = PROJECT_ROOT / "Learning" / "Images"

def main():
    IMAGES_DIR.mkdir(parents=True, exist_ok=True)
    html_url = MOCKUP_FILE.as_uri()

    print(f"Loading mockup: {html_url}")
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        # iPhone 14 Pro viewport
        page = browser.new_page(viewport={"width": 430, "height": 950}, device_scale_factor=2)
        page.goto(html_url, wait_until="networkidle")

        # 1. Full device frame overview
        phone_frame = page.locator("#app-frame")
        phone_frame.screenshot(path=str(IMAGES_DIR / "MainPage-overview.png"))
        print("Captured: MainPage-overview.png")

        # 2. Hero Banner
        hero = page.locator(".hero-banner")
        hero.screenshot(path=str(IMAGES_DIR / "MainPage-hero.png"))
        print("Captured: MainPage-hero.png")

        # 3. Block 1: Playlist
        b1 = page.locator(".block-1")
        b1.screenshot(path=str(IMAGES_DIR / "MainPage-block1-playlist.png"))
        print("Captured: MainPage-block1-playlist.png")

        # 4. Block 2: Stats
        b2 = page.locator(".block-2")
        b2.screenshot(path=str(IMAGES_DIR / "MainPage-block2-stats.png"))
        print("Captured: MainPage-block2-stats.png")

        # 5. Block 3: Add Video Form
        b3 = page.locator(".block-3")
        b3.screenshot(path=str(IMAGES_DIR / "MainPage-block3-addvideo.png"))
        print("Captured: MainPage-block3-addvideo.png")

        # 6. Block 4: Filter & Sort Bar
        b4 = page.locator(".block-4")
        b4.screenshot(path=str(IMAGES_DIR / "MainPage-block4-filter.png"))
        print("Captured: MainPage-block4-filter.png")

        # 7. Block 5: CollectionView Video Cards
        b5 = page.locator(".block-5")
        b5.screenshot(path=str(IMAGES_DIR / "MainPage-block5-cards.png"))
        print("Captured: MainPage-block5-cards.png")

        browser.close()
        print("All block screenshots captured successfully!")

if __name__ == "__main__":
    main()
