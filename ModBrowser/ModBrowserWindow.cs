using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MinesweeperModAPI.Interfaces;
using MinesweeperModAPI.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;

public sealed class ModBrowserWindow : Window
{
    private static readonly ResourceManager resources =
        new("ModBrowser.Localization.Strings", typeof(ModBrowserWindow).Assembly);
    private static readonly CultureInfo language = GetLanguageCulture();

    private readonly IModManager modManager;
    private readonly StackPanel modList = new()
    {
        Spacing = 8
    };
    private readonly ContentControl pageContent = new();
    private readonly TextBox searchBox = new()
    {
        PlaceholderText = GetText("SearchPlaceholder")
    };
    private readonly TextBlock countText = new()
    {
        Foreground = Brushes.Gray,
        VerticalAlignment = VerticalAlignment.Center
    };

    public ModBrowserWindow(IModManager modManager)
    {
        this.modManager = modManager;

        Title = GetText("Title");
        Width = 800;
        Height = 550;
        MinWidth = 600;
        MinHeight = 350;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        searchBox.TextChanged += (_, _) => RefreshMods();

        var refreshButton = new Button
        {
            Content = GetText("Refresh")
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
                            Text = GetText("Installed"),
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
            Content = GetText("Installed"),
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        installedButton.Click += (_, _) => pageContent.Content = installedPage;

        var discoverButton = new Button
        {
            Content = GetText("Discover"),
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        discoverButton.Click += (_, _) => ShowComingSoon();

        var settingsButton = new Button
        {
            Content = GetText("Settings"),
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
                        Text = GetText("Menu"),
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
                    Text = GetText("ComingSoon"),
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

        countText.Text = string.Format(
            language,
            GetText(mods.Count == 1 ? "OneModCount" : "ManyModsCount"),
            mods.Count);

        if (mods.Count == 0)
        {
            modList.Children.Add(new TextBlock
            {
                Text = GetText("NoModsFound"),
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
                CreateDetail(GetText("Id"), info.Id),
                CreateDetail(GetText("Version"), info.Version),
                CreateDetail(GetText("Author"), info.Author),
                CreateDetail(GetText("Licence"), info.LicenceType),
                CreateDetail(GetText("LicenceInformation"), info.LicenceInfo, isLink: true)
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

    private static Control CreateDetail(string label, string value, bool isLink = false)
    {
        var valueText = new TextBlock
        {
            Text = value,
            Foreground = Brushes.Gray,
            TextWrapping = TextWrapping.Wrap
        };

        if (isLink && Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            valueText.Foreground = Brushes.CornflowerBlue;
            valueText.PointerEntered += (_, _) =>
            {
                valueText.TextDecorations = new TextDecorationCollection
                {
                    new TextDecoration
                    {
                        Location = TextDecorationLocation.Underline
                    }
                };
            };
            valueText.PointerExited += (_, _) =>
            {
                valueText.TextDecorations = null;
            };
            valueText.PointerPressed += async (_, e) =>
            {
                e.Handled = true;

                var topLevel = TopLevel.GetTopLevel(valueText);
                if (topLevel is not null)
                    await topLevel.Launcher.LaunchUriAsync(uri);
            };
        }

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
                valueText
            }
        };
    }

    private static string GetText(string name) =>
        resources.GetString(name, language) ??
        throw new InvalidOperationException($"Missing localization string '{name}'.");

    private static CultureInfo GetLanguageCulture()
    {
        string applicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(applicationData))
        {
            throw new InvalidOperationException("The local application data directory is unavailable.");
        }

        string settingsPath = Path.Combine(applicationData, "MinesweeperReborn", "language.txt");
        string preference = File.Exists(settingsPath)
            ? File.ReadAllText(settingsPath).Trim()
            : "System";

        if (preference.Equals("English", StringComparison.OrdinalIgnoreCase))
            return CultureInfo.GetCultureInfo("en-US");
        if (preference.Equals("Hungarian", StringComparison.OrdinalIgnoreCase))
            return CultureInfo.GetCultureInfo("hu-HU");
        if (!preference.Equals("System", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Invalid language preference in '{settingsPath}'.");

        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en"
            ? CultureInfo.GetCultureInfo("en-US")
            : CultureInfo.GetCultureInfo("hu-HU");
    }
}
