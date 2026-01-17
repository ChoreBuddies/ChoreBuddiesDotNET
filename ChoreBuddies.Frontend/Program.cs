using Blazored.LocalStorage;
using ChoreBuddies.Frontend.Features.Authentication;
using ChoreBuddies.Frontend.Features.Chat;
using ChoreBuddies.Frontend.Features.Chores;
using ChoreBuddies.Frontend.Features.ExceptionHandler;
using ChoreBuddies.Frontend.Features.Household;
using ChoreBuddies.Frontend.Features.Notifications;
using ChoreBuddies.Frontend.Features.PredefinedChores;
using ChoreBuddies.Frontend.Features.RedeemedRewards;
using ChoreBuddies.Frontend.Features.Reminders;
using ChoreBuddies.Frontend.Features.Rewards;
using ChoreBuddies.Frontend.Features.ScheduledChores;
using ChoreBuddies.Frontend.Features.User;
using ChoreBuddies.Frontend.UI;
using ChoreBuddies.Frontend.UI.Services;
using ChoreBuddies.Frontend.Utilities;
using ChoreBuddies.Frontend.Utilities.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

namespace ChoreBuddies.Frontend;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            config.SnackbarConfiguration.PreventDuplicates = false;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 5000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });
        builder.Services.AddSingleton<ISnackbar, SnackbarService>();
        builder.Services.AddScoped<ToastService>();
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

        builder.Services.AddBlazoredLocalStorage();

        // Authorization
        builder.Services.AddScoped<JwtAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<JwtAuthStateProvider>());
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Chores
        builder.Services.AddScoped<IChoresService, ChoresService>();
        builder.Services.AddScoped<IScheduledChoresService, ScheduledChoresService>();
        builder.Services.AddScoped<IPredefinedChoresService, PredefinedChoresService>();

        // Rewards
        builder.Services.AddScoped<IRewardsService, RewardsService>();
        builder.Services.AddScoped<IRedeemedRewardsService, RedeemedRewardsService>();

        // Other services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IHouseholdService, HouseholdService>();
        builder.Services.AddScoped<INotificationsService, NotificationsService>();
        builder.Services.AddScoped<IRemindersService, RemindersService>();

        builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        // Register the custom HttpMessageHandler and configure the HttpClient
        builder.Services.AddTransient<AuthorizedHttpClient>();
        builder.Services.AddTransient<ApiExceptionInterceptor>();

        var apiUrl = builder.Configuration[AppConstants.ApiUrl] ?? AppConstants.DefaultApiUrl;
        builder.Services.AddHttpClient(AuthFrontendConstants.AuthorizedClient, client =>
        {
            client.BaseAddress = new Uri(apiUrl);
        }).AddHttpMessageHandler<ApiExceptionInterceptor>() // This checks from API exceptions (must be before AuthorizedHttpClient)
          .AddHttpMessageHandler<AuthorizedHttpClient>(); // This adds the auth header 

        builder.Services.AddHttpClient(AuthFrontendConstants.UnauthorizedClient, client =>
        {
            client.BaseAddress = new Uri(apiUrl);
        }).AddHttpMessageHandler<ApiExceptionInterceptor>();

        builder.Services.AddScoped<HttpClientUtils>();
        builder.Services.AddScoped<IAuthApiService, AuthApiService>();

        // Chat service
        builder.Services.AddScoped<IChatService, ChatService>();

        builder.Services.AddAuthorizationCore();

        var app = builder.Build();

        await app.RunAsync();
    }

}

