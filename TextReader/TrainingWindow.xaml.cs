using System.Windows;
using TextReader.Services;

namespace TextReader;

public partial class TrainingWindow : Window
{
    private readonly NotebookService _service;

    public TrainingWindow(NotebookService service)
    {
        InitializeComponent();
        _service = service;

        LoadWord();
    }

    private void LoadWord()
    {
        if (_service.IsFinished)
        {
            MessageBox.Show("Тренировка завершена!");
            Close();
            return;
        }

        var word = _service.CurrentWord;

        WordText.Text = word?.Word;
        TranslationText.Text = word?.Translation;
    }

    private void Remember_Click(object sender, RoutedEventArgs e)
    {
        _service.Remember();
        LoadWord();
    }

    private void Forgot_Click(object sender, RoutedEventArgs e)
    {
        _service.Forgot();
        LoadWord();
    }
}