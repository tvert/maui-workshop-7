using Microsoft.Extensions.Logging;
using MonkeyFinder.Services;
using MonkeyFinder.View;

namespace MonkeyFinder;

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
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        // Registration for Dependency Injection
        {
            // Register Service(s)
            builder.Services.AddSingleton<MonkeyService>();

            // Register ViewModel(s)
            builder.Services.AddSingleton<MonkeysViewModel>();

            // Register View(s) (aka Page(s))
            builder.Services.AddSingleton<MainPage>();
        }

        return builder.Build();
    }
}
