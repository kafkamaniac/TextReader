namespace TextReader.ReadingEngine;

public class PageManager
{
    public event Action<int> OnPageChanged;

    private readonly ReadingDocument _document;
    private readonly int _pageSize;

    private readonly List<PageModel> _pages = new();

    public IReadOnlyList<PageModel> Pages => _pages;

    public int PageCount => _pages.Count;
    private int _currentPage;

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (_currentPage == value)
                return;

            _currentPage = value;
            OnPageChanged?.Invoke(_currentPage);
        }
    }

    public PageModel Current =>
    _pages.Count == 0 ? new PageModel() : _pages[CurrentPage];

    public PageManager(ReadingDocument document, int pageSize = 20)
    {
        _document = document;
        _pageSize = pageSize;

        BuildPages();
    }

    private void BuildPages()
    {
        _pages.Clear();

        int index = 0;

        while (index < _document.Paragraphs.Count)
        {
            var page = new PageModel
            {
                Index = _pages.Count,
                StartIndex = index
            };

            int end = Math.Min(index + _pageSize, _document.Paragraphs.Count);

            for (int i = index; i < end; i++)
            {
                page.Paragraphs.Add(_document.Paragraphs[i]);
            }

            page.EndIndex = end - 1;

            _pages.Add(page);

            index += _pageSize;
        }
    }

    public PageModel GetCurrentPage()
    {
        if (_pages.Count == 0)
            return new PageModel();

        return _pages[CurrentPage];
    }

    public bool NextPage()
    {
        if (CurrentPage + 1 >= _pages.Count)
            return false;

        CurrentPage++;
        return true;
    }

    public bool PreviousPage()
    {
        if (CurrentPage - 1 < 0)
            return false;

        CurrentPage--;
        return true;
    }

    public void SetPage(int index)
    {
        if (index < 0 || index >= _pages.Count)
            return;

        CurrentPage = index;
    }

    public bool GoToPage(int index)
    {
        if (index < 0 || index >= _pages.Count)
            return false;

        _currentPage = index;
        OnPageChanged?.Invoke(_currentPage);
        return true;
    }
}