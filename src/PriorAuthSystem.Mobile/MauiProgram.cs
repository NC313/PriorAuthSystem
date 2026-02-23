using Microsoft.Extensions.Logging;
using PriorAuthSystem.Mobile.Services;
using PriorAuthSystem.Mobile.ViewModels;
using PriorAuthSystem.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace PriorAuthSystem.Mobile;

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

		// HttpClient
		builder.Services.AddHttpClient<PriorAuthApiService>(client =>
		{
			client.BaseAddress = new Uri("http://localhost:5000/api/PriorAuthorizations/");
		});

		// ViewModels
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<DetailViewModel>();

		// Pages
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<DetailPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
