using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.IO;



namespace ReserveNow;

public static class MauiProgram
{
    public static IConfiguration Configuration { get; private set; }
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

        var assembly = typeof(MauiProgram).Assembly;
        var resourceName = "ReserveNow.appsettings.json"; // Укажите полное имя ресурса

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Configuration file '{resourceName}' not found.");
        }

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        Configuration = config;
#if DEBUG
        //builder.Services.AddSingleton(config);
        builder.Services.AddSingleton<HttpClient>();

        // Регистрация сервисов
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<MainPage>();
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
