using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ChoreBuddies.Frontend.UI.Services;

public class ToastService(ISnackbar snackbar)
{
    private readonly ISnackbar _snackbar = snackbar;

    public void ShowError(string title, string message)
    {
        _snackbar.Add(
            new MarkupString($"<strong>{title}</strong><br/>{message}"),
            Severity.Error,
            config =>
            {
                config.ShowCloseIcon = true;
                config.CloseAfterNavigation = false;
            });
    }

    public void ShowSuccess(string message)
    {
        _snackbar.Add(message, Severity.Success);
    }
}
