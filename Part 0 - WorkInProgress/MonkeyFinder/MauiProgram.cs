﻿using Microsoft.Extensions.Logging;
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
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IMap>(Map.Default);


            // Register ViewModel(s)
            builder.Services.AddSingleton<MonkeysViewModel>();
            builder.Services.AddTransient<MonkeyDetailsViewModel>();

            // Register View(s) (aka Page(s))
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<DetailsPage>();
        }

        return builder.Build();
    }
}
