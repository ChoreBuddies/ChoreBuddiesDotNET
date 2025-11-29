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

        try
        {
            // Build the HubConnection (if it doesn't exist yet)
            if (_hubConnection is null)
            {
                var baseUrl = configuration[AppConstants.ApiUrl] ?? AppConstants.DefaultApiUrl;
                var hubUrl = $"{baseUrl.TrimEnd('/')}/chatHub";

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options =>
                    {
                        options.AccessTokenProvider = async () =>
                        {
                            var token = await authService.GetTokenAsync();
                            return token;
                        };
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // Register event handlers
                _hubConnection.On<ChatMessageDto>(ChatConstants.ReceiveMessage, (msg) => MessageReceived?.Invoke(msg));
                _hubConnection.On<string>(ChatConstants.UserIsTyping, (user) => UserTyping?.Invoke(user));

                _hubConnection.Closed += (ex) =>
                {
                    if (ex != null) Console.Error.WriteLine($"[SignalR] Connection closed with error: {ex.Message}");
                    return Task.CompletedTask;
                };
            }

            // Start the connection (if not connected)
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }

            //  Join the household chat group
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync(ChatConstants.JoinHouseholdChat, householdId);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"[SignalR] Error with network/authorization: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[SignalR] General error: {ex.Message}");
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection is not null)
        {
            try
            {
                await _hubConnection.StopAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[SignalR] Error with disconnecting: {ex.Message}");
            }
            finally
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
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
        await TryHubInvokeAsync(ChatConstants.SendMessage, householdId, content, clientUniqueId);
    }

    public async Task SendTypingAsync(int householdId)
    {
        await TryHubInvokeAsync(ChatConstants.SendTyping, householdId);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    private async Task TryHubInvokeAsync(string methodName, params object[] args)
    {
        if (_hubConnection is null || !IsConnected)
        {
            return;
        }

        try
        {
            await _hubConnection.InvokeCoreAsync(methodName, args);
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"[SignalR] Error with network/authorization '{methodName}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[SignalR] General error in '{methodName}': {ex.Message}");
        }
    }
}
