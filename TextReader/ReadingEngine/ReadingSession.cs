using System.Windows.Documents;

namespace TextReader.ReadingEngine;

public class ReadingSession
{
    private PageManager _pageManager;

    public int CurrentPageIndex => _pageManager.CurrentPage;
    public int TotalPages => _pageManager.Pages.Count;

    public void Open(ReadingDocument doc)
    {
        _pageManager = new PageManager(doc);
    }

    public FlowDocument GetCurrentPage()
    {
        return PageRenderer.Render(_pageManager.Current);
    }

    public bool NextPage()
    {
        return _pageManager.NextPage();
    }

    public bool PreviousPage()
    {
        return _pageManager.PreviousPage();
    }

    public bool GoToPage(int index)
    {
        if (index < 0 || index >= _pageManager.Pages.Count)
            return false;

        _pageManager.SetPage(index); // добавим ниже
        return true;
    }


}