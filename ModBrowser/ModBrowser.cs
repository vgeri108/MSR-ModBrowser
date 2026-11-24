
using Avalonia.Controls;
using Avalonia.Threading;
using MinesweeperModAPI;
using MinesweeperModAPI.Models;

public sealed class ModBrowser : Mod
{
    private ModBrowserWindow? window;
    private IDisposable? menuItem;

    public override ModInfo Info => new()
    {
        Id = "mod.vgeri.modbrowser",
        Name = "Mod Browser",
        Version = "1.0.3",
        Author = "vgeri108",
        LicenceType = "MIT",
        LicenceInfo = "https://opensource.org/license/mit/"
    };

    public override void OnEnable()
    {
        menuItem = Context.AddMenuItem("settings", Info.Name, OpenWindow);
    }

    private void OpenWindow()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (window is not null)
                return;

            window = new ModBrowserWindow(Context.ModManager);

            window.Closed += (_, _) => window = null;

            window.Show();
        });
    }

    public override void OnDisable()
    {
        Dispatcher.UIThread.Post(() =>
        {
            menuItem?.Dispose();
            menuItem = null;

            ModBrowserWindow? current = window;
            window = null;

            current?.Close();
        });
    }
}
