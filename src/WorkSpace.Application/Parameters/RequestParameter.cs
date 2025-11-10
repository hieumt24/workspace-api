namespace WorkSpace.Application.Parameters;

public class RequestParameter
{
    private const int MaxPageSize = 100;
    private int _pageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = (value < 1) ? 1 : value;
    }
    public int PageSize { get; set; }

    private int _pageSize = 10;
    public int Size
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 10 : value);
    }
}