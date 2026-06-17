using System.Windows;
using System.Windows.Media;

namespace TextReader.Services;

public static class ThemeService
{
    public static void ApplyTheme(string themeName)
    {
        switch (themeName)
        {
            case "Dark":
                Application.Current.Resources["WindowBackground"] =
                    new SolidColorBrush(Color.FromRgb(30, 30, 30));

                Application.Current.Resources["EditorBackground"] =
                    new SolidColorBrush(Color.FromRgb(45, 45, 45));

                Application.Current.Resources["EditorForeground"] =
                    Brushes.White;
                break;

            case "Sepia":
                Application.Current.Resources["WindowBackground"] =
                    new SolidColorBrush(Color.FromRgb(244, 236, 220));

                Application.Current.Resources["EditorBackground"] =
                    new SolidColorBrush(Color.FromRgb(251, 240, 217));

                Application.Current.Resources["EditorForeground"] =
                    Brushes.Black;
                break;

            default:
                Application.Current.Resources["WindowBackground"] =
                    Brushes.White;

                Application.Current.Resources["EditorBackground"] =
                    Brushes.White;

                Application.Current.Resources["EditorForeground"] =
                    Brushes.Black;
                break;
        }
    }
}