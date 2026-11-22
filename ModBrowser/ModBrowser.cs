
using Avalonia.Controls;
using Avalonia.Threading;
using MinesweeperModAPI;
using MinesweeperModAPI.Models;

public sealed class ModBrowser : Mod
{
    private ModBrowserWindow? window;

    public override ModInfo Info => new()
    {
        Id = "mod.vgeri.modbrowser",
        Name = "Mod Browser",
        Version = "1.0.0",
        Author = "vgeri108",
        LicenceType = "MIT",
        LicenceInfo = "https://opensource.org/license/mit/"
    };

    public override void OnEnable()
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
            ModBrowserWindow? current = window;
            window = null;

            current?.Close();
        });
    }
}
