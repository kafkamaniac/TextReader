using DocumentFormat.OpenXml.Drawing.Charts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static TextReader.MainWindow;
using TextReader.Services;

namespace TextReader;

public partial class SettingsWindow : Window
{
    private bool _isLoaded;
    public AppMode SelectedMode { get; private set; } = AppMode.Edit;

    public SettingsWindow()
    {
        InitializeComponent();

        Loaded += (_, __) =>
        {
            _isLoaded = true;
            ThemeBox.SelectedIndex = 0;
        };
    }

    private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isLoaded)
            return;

        if (CategoryList.SelectedItem is not ListBoxItem item)
            return;

        string category = item.Content?.ToString() ?? "";

        AppearancePanel.Visibility = Visibility.Collapsed;
        ReadingPanel.Visibility = Visibility.Collapsed;
        LanguagePanel.Visibility = Visibility.Collapsed;

        switch (item.Tag?.ToString())
        {
            case "Appearance":
                AppearancePanel.Visibility = Visibility.Visible;
                break;

            case "Reading":
                ReadingPanel.Visibility = Visibility.Visible;
                break;

            case "Language":
                LanguagePanel.Visibility = Visibility.Visible;
                break;
        }
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (ThemeBox.SelectedItem is not ComboBoxItem item)
            return;

        switch (item.Tag?.ToString())
        {
            case "Dark":
                ApplyDarkTheme();
                break;

            case "Sepia":
                ApplySepiaTheme();
                break;

            default:
                ApplyLightTheme();
                break;
        }
    }
    private void ApplyLightTheme()
    {
        Application.Current.Resources["WindowBackground"] = Brushes.White;
        Application.Current.Resources["EditorBackground"] = Brushes.White;
        Application.Current.Resources["EditorForeground"] = Brushes.Black;
    }

    private void ApplyDarkTheme()
    {
        Application.Current.Resources["WindowBackground"] =
            new SolidColorBrush(Color.FromRgb(30, 30, 30));

        Application.Current.Resources["EditorBackground"] =
            new SolidColorBrush(Color.FromRgb(45, 45, 45));

        Application.Current.Resources["EditorForeground"] =
            Brushes.White;
    }

    private void ApplySepiaTheme()
    {
        Application.Current.Resources["WindowBackground"] =
            new SolidColorBrush(Color.FromRgb(244, 236, 220));

        Application.Current.Resources["EditorBackground"] =
            new SolidColorBrush(Color.FromRgb(251, 240, 217));

        Application.Current.Resources["EditorForeground"] =
            Brushes.Black;
    }



    private void ApplyReading_Click(object sender, RoutedEventArgs e)
    {
        string mode = (ReadingModeBox.SelectedItem as ComboBoxItem)?
                      .Content?.ToString() ?? "";

        SelectedMode = mode switch
        {
            "Страницы" => AppMode.ReadPages,
            "Книга" => AppMode.ReadBook,
            "Прокрутка" => AppMode.ReadScroll,
            _ => AppMode.Edit
        };

        DialogResult = true;
    }

    private void ApplyLanguage_Click(object sender, RoutedEventArgs e)
    {
        string language =
            (LanguageBox.SelectedItem as ComboBoxItem)?
            .Content?.ToString() ?? "Русский";

        LanguageService.ChangeLanguage(language);
        Application.Current.MainWindow.Dispatcher.Invoke(() =>
        {
            ((MainWindow)Application.Current.MainWindow).UpdateModeButton();
        });
    }

}