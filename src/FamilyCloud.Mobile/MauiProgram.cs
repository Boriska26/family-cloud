using FamilyCloud.Mobile.Services;
using FamilyCloud.Mobile.ViewModels;
using FamilyCloud.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace FamilyCloud.Mobile;

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

#if ANDROID
		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(
			"RemoveUnderline",
			(handler, view) =>
			{
				handler.PlatformView.BackgroundTintList =
					Android.Content.Res.ColorStateList.ValueOf(
						Android.Graphics.Color.Transparent);
			});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services.AddHttpClient<AuthService>(client =>
		{
			client.BaseAddress = new Uri("http://192.168.0.19:8080/");
			client.Timeout = TimeSpan.FromSeconds(15);
		});

		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<LoginPage>();

		return builder.Build();
	}
}
