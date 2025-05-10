using Microsoft.Extensions.Logging;



namespace ReserveNow;

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
#if DEBUG
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
