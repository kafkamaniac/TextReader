using System.Windows.Documents;
using TextReader.ReadingEngine;

public class ReadingSession
{
    private PageManager _pageManager;

    public event Action<int>? OnPageChanged;

    public int CurrentPageIndex => _pageManager.CurrentPage;
    public int TotalPages => _pageManager.PageCount;

    public void Open(ReadingDocument doc)
    {
        _pageManager = new PageManager(doc);
        OnPageChanged?.Invoke(_pageManager.CurrentPage);
    }

    public FlowDocument GetCurrentPage()
        => PageRenderer.Render(_pageManager.Current);

    public bool NextPage()
    {
        if (_pageManager.NextPage())
        {
            OnPageChanged?.Invoke(_pageManager.CurrentPage);
            return true;
        }
        return false;
    }

    public bool PreviousPage()
    {
        if (_pageManager.PreviousPage())
        {
            OnPageChanged?.Invoke(_pageManager.CurrentPage);
            return true;
        }
        return false;
    }

    public bool GoToPage(int index)
    {
        if (_pageManager == null)
            return false;

        if (index < 0 || index >= _pageManager.PageCount)
            return false;

        _pageManager.SetPage(index);
        OnPageChanged?.Invoke(_pageManager.CurrentPage);
        return true;
    }
}