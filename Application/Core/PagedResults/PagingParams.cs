using System;

namespace Application.Core.PagedResults;

public class PagingParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int Skip => (PageNumber - 1) * PageSize;

    public void Normalize(int maxPageSize = 100)
    {
        if(PageNumber < 1) PageNumber = 1;
        if(PageSize < 1) PageSize = 10;
        if(PageSize > maxPageSize) PageSize = maxPageSize;
    }
}
