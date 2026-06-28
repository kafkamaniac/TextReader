using TextReader.Models;

namespace TextReader.Services;

public class NotebookService
{
    private readonly Random _random = new();

    private List<VocabularyItem> _trainingQueue = new();

    public VocabularyItem? CurrentWord =>
        _trainingQueue.Count > 0
            ? _trainingQueue[0]
            : null;

    public bool IsFinished =>
        _trainingQueue.Count == 0;

    public void StartTraining()
    {
        _trainingQueue = VocabularyBook.Items.ToList();

        foreach (var item in _trainingQueue)
        {
            item.SuccessCount = 0;
        }
    }

    public void Remember()
    {
        if (CurrentWord == null)
            return;

        var item = CurrentWord;

        item.SuccessCount++;

        _trainingQueue.RemoveAt(0);

        if (item.SuccessCount >= 3)
        {
            return;
        }

        _trainingQueue.Add(item);
    }

    public void Forgot()
    {
        if (CurrentWord == null)
            return;

        var item = CurrentWord;

        item.SuccessCount = 0;

        _trainingQueue.RemoveAt(0);

        int maxPosition =
            Math.Min(10, _trainingQueue.Count);

        int position =
            _random.Next(0, maxPosition + 1);

        _trainingQueue.Insert(position, item);
    }
}