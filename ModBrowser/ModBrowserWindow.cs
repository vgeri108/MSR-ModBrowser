using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MinesweeperModAPI.Interfaces;
using MinesweeperModAPI.Models;
using System;
using System.Linq;

public sealed class ModBrowserWindow : Window
{
    private readonly IModManager modManager;
    private readonly StackPanel modList = new()
    {
        Spacing = 8
    };
    private readonly ContentControl pageContent = new();
    private readonly TextBox searchBox = new()
    {
        PlaceholderText = "Search mods..."
    };
    private readonly TextBlock countText = new()
    {
        Foreground = Brushes.Gray,
        VerticalAlignment = VerticalAlignment.Center
    };

    public ModBrowserWindow(IModManager modManager)
    {
        this.modManager = modManager;

        Title = "Mod Browser";
        Width = 800;
        Height = 550;
        MinWidth = 600;
        MinHeight = 350;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        searchBox.TextChanged += (_, _) => RefreshMods();

        var refreshButton = new Button
        {
            Content = "Refresh"
        };
        refreshButton.Click += (_, _) => RefreshMods();

        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        footer.Children.Add(countText);
        footer.Children.Add(refreshButton);
        Grid.SetColumn(refreshButton, 1);

        var installedPage = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            RowSpacing = 14,
            Margin = new Thickness(18),
            Children =
            {
                new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Installed",
                            FontSize = 25,
                            FontWeight = FontWeight.Bold
                        },
                        searchBox
                    }
                },
                new ScrollViewer
                {
                    Content = modList,
                    VerticalScrollBarVisibility =
                        Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
                },
                footer
            }
        };
        Grid.SetRow(installedPage.Children[1], 1);
        Grid.SetRow(footer, 2);

        var installedButton = new Button
        {
            Content = "Installed",
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        installedButton.Click += (_, _) => pageContent.Content = installedPage;

        var discoverButton = new Button
        {
            Content = "Discover",
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        discoverButton.Click += (_, _) => ShowComingSoon();

        var settingsButton = new Button
        {
            Content = "Settings",
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        settingsButton.Click += (_, _) => ShowComingSoon();

        var sidebar = new Border
        {
            Padding = new Thickness(12),
            BorderThickness = new Thickness(0, 0, 1, 0),
            BorderBrush = Brushes.Gray,
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Menu",
                        FontSize = 18,
                        FontWeight = FontWeight.Bold,
                        Margin = new Thickness(4, 4, 4, 12)
                    },
                    installedButton,
                    discoverButton,
                    settingsButton
                }
            }
        };

        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("150,*"),
            Children =
            {
                sidebar,
                pageContent
            }
        };
        Grid.SetColumn(pageContent, 1);
        Content = layout;
        pageContent.Content = installedPage;

        RefreshMods();
    }

    private void ShowComingSoon()
    {
        pageContent.Content = new Grid
        {
            Children =
            {
                new TextBlock
                {
                    Text = "Coming soon",
                    FontSize = 24,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private void RefreshMods()
    {
        modList.Children.Clear();

        string search = searchBox.Text?.Trim() ?? "";

        var mods = modManager.GetMods()
            .Where(mod =>
            {
                ModInfo info = mod.Info;
                return info.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    info.Author.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    info.Id.Contains(search, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();

        foreach (var mod in mods)
            modList.Children.Add(CreateModCard(mod));

        countText.Text = $"{mods.Count} mods";

        if (mods.Count == 0)
        {
            modList.Children.Add(new TextBlock
            {
                Text = "No mods found.",
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Gray,
                Margin = new Thickness(15)
            });
        }
    }

    private Control CreateModCard(ModEntry mod)
    {
        ModInfo info = mod.Info;

        var name = new TextBlock
        {
            Text = info.Name,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold
        };

        var metadata = new TextBlock
        {
            Text = $"v{info.Version}  •  {info.Author}",
            FontSize = 12,
            Foreground = Brushes.Gray
        };

        var infoPanel = new StackPanel
        {
            Spacing = 4,
            Children = { name, metadata }
        };

        var details = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(0, 12, 0, 0),
            IsVisible = false,
            Children =
            {
                CreateDetail("ID", info.Id),
                CreateDetail("Version", info.Version),
                CreateDetail("Author", info.Author),
                CreateDetail("Licence", info.LicenceType),
                CreateDetail("Licence information", info.LicenceInfo)
            }
        };

        var card = new Border
        {
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Gray,
            Background = Brushes.Transparent,
            Child = new StackPanel
            {
                Children = { infoPanel, details }
            }
        };
        card.PointerPressed += (_, _) => details.IsVisible = !details.IsVisible;
        return card;
    }

    private static Control CreateDetail(string label, string value)
    {
        return new StackPanel
        {
            Spacing = 3,
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    FontWeight = FontWeight.Bold
                },
                new TextBlock
                {
                    Text = value,
                    Foreground = Brushes.Gray,
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
    }
}
