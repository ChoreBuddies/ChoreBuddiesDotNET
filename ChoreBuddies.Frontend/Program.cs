using Blazored.LocalStorage;
using ChoreBuddies.Frontend.Features.Authentication;
using ChoreBuddies.Frontend.UI;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace ChoreBuddies.Frontend;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

        builder.Services.AddBlazoredLocalStorage();

        // Authorization
        builder.Services.AddScoped<JwtAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<JwtAuthStateProvider>());
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Register the custom HttpMessageHandler and configure the HttpClient
        builder.Services.AddTransient<AuthorizedHttpClient>();
        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthorizedClient"));
        var apiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:7014";
        builder.Services.AddHttpClient("AuthorizedClient", client =>
        {
            client.BaseAddress = new Uri(apiUrl);
        }).AddHttpMessageHandler<AuthorizedHttpClient>(); // This adds the auth header to all requests made by this client

        builder.Services.AddHttpClient("UnauthorizedClient", client =>
        {
            client.BaseAddress = new Uri(apiUrl);
        });

        builder.Services.AddAuthorizationCore();

        var app = builder.Build();

        await app.RunAsync();
    }

}

