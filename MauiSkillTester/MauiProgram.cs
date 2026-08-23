using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using MauiSkillTester.Services;
using MauiSkillTester.ViewModels;

namespace MauiSkillTester;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // רישום שכבת הנתונים, ה-ViewModel וה-View במכולת התלויות (DI Container)
        builder.Services.AddSingleton<IDatabaseService, SQLiteDatabaseService>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        // אתחול ספקי SQLite הנייטיביים עבור כל הפלטפורמות
        Batteries_V2.Init();

        return builder.Build();
    }
}
