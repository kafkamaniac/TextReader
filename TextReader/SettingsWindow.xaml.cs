using DocumentFormat.OpenXml.Drawing.Charts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static TextReader.MainWindow;
using TextReader.Services;
using System.Speech.Synthesis;

namespace TextReader;

public partial class SettingsWindow : Window
{
    private bool _isLoaded;
    private SpeechSynthesizer _synth = new SpeechSynthesizer();
    public AppMode SelectedMode { get; private set; } = AppMode.Edit;

    public string SelectedTheme { get; set; } = "Light";

    public string SelectedVoice { get; set; }
    public int SelectedRate { get; set; }

    public SettingsWindow()
    {
        InitializeComponent();

        Loaded += (_, __) =>
        {
            _isLoaded = true;
            ThemeBox.SelectedIndex = 0;
        };

        LoadVoices();
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
        SpeechPanel.Visibility = Visibility.Collapsed;

        switch (item.Tag?.ToString())
        {
            case "Appearance":
                AppearancePanel.Visibility = Visibility.Visible;
                break;

            case "ReadingMode":
                ReadingPanel.Visibility = Visibility.Visible;
                break;

            case "Language":
                LanguagePanel.Visibility = Visibility.Visible;
                break;

            case "Speech":
                SpeechPanel.Visibility = Visibility.Visible;
                break;
        }
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        string theme =
            (ThemeBox.SelectedItem as ComboBoxItem)?
            .Tag?.ToString() ?? "Light";

        ApplyTheme(theme);

        DialogResult = true;
    }

    private void ApplyTheme(string themeName)
    {
        SelectedTheme = themeName;
        ThemeService.ApplyTheme(themeName);
    }


    private void ApplyReading_Click(object sender, RoutedEventArgs e)
    {
        string mode = (ReadingModeBox.SelectedItem as ComboBoxItem)?
                      .Tag?.ToString() ?? "";

        SelectedMode = mode switch
        {
            "Page" => AppMode.ReadPages,
            "TwoPage" => AppMode.ReadBook,
            "Scroll" => AppMode.ReadScroll,
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

    private void LoadVoices()
    {
        foreach (var voice in _synth.GetInstalledVoices())
        {
            VoiceBox.Items.Add(new ComboBoxItem
            {
                Content = voice.VoiceInfo.Name,
                Tag = voice.VoiceInfo.Name
            });
        }
        if (VoiceBox.Items.Count > 0)
            VoiceBox.SelectedIndex = 0;
    }

    private void ApplySpeech_Click(object sender, RoutedEventArgs e)
    {
        SelectedVoice =
     (VoiceBox.SelectedItem as ComboBoxItem)?
     .Tag?.ToString() ?? "";

        SelectedRate = (int)SpeedSlider.Value;

        DialogResult = true;
    }

}