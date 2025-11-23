using ChoreBuddies.Frontend.Features.Authentication;
using ChoreBuddies.Frontend.Utilities;
using ChoreBuddies.Frontend.Utilities.Constants;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Chat;

namespace ChoreBuddies.Frontend.Features.Chat;

public interface IChatService : IAsyncDisposable
{
    bool IsConnected { get; }

    event Action<ChatMessageDto>? MessageReceived;
    event Action<string>? UserTyping;

    Task ConnectAsync(int householdId);
    Task DisconnectAsync();
    Task<List<ChatMessageDto>> GetHistoryAsync(int householdId);
    Task SendMessageAsync(int householdId, string content, Guid clientUniqueId);
    Task SendTypingAsync(int householdId);
}

public class ChatService(
    HttpClientUtils httpUtils,
    IAuthService authService,
    IConfiguration configuration) : IChatService
{
    private HubConnection? _hubConnection;

    public event Action<ChatMessageDto>? MessageReceived;
    public event Action<string>? UserTyping;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public async Task ConnectAsync(int householdId)
    {
        if (_hubConnection is not null && IsConnected) return;

        // Build the HubConnection
        var baseUrl = configuration[AppConstants.ApiUrl] ?? AppConstants.DefaultApiUrl;
        var hubUrl = $"{baseUrl.TrimEnd('/')}/chatHub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    return await authService.GetTokenAsync();
                };
            })
            .WithAutomaticReconnect()
            .Build();

        // Register event handlers
        _hubConnection.On<ChatMessageDto>("ReceiveMessage", (msg) => MessageReceived?.Invoke(msg));
        _hubConnection.On<string>("UserIsTyping", (user) => UserTyping?.Invoke(user));

        // Start the connection
        await _hubConnection.StartAsync();

        // Join the household chat group
        await _hubConnection.InvokeAsync("JoinHouseholdChat", householdId);
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async Task<List<ChatMessageDto>> GetHistoryAsync(int householdId)
    {
        var result = await httpUtils.TryRequestAsync(async () =>
        {
            return await httpUtils.GetAsync<List<ChatMessageDto>>(
                $"api/v1/chat/{householdId}",
                authorized: true
            );
        });

        return result ?? [];
    }

    public async Task SendMessageAsync(int householdId, string content, Guid clientUniqueId)
    {
        if (_hubConnection is not null && IsConnected)
        {
            await _hubConnection.InvokeAsync("SendMessage", householdId, content, clientUniqueId);
        }
    }

    public async Task SendTypingAsync(int householdId)
    {
        if (_hubConnection is not null && IsConnected)
        {
            await _hubConnection.InvokeAsync("SendTyping", householdId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
