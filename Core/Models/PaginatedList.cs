using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalRecords { get; private set; }


        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        private PaginatedList(IEnumerable<T> items, int count, int pageIndex, int pageSize)
        {
            TotalRecords = count;
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public static PaginatedList<T> Create(List<T>? items, int count, FilterOptions filter)
        {
            return items is null ? new PaginatedList<T>(Enumerable.Empty<T>().ToList(), 0, 0, 0) : new(items, count, filter.PageIndex, filter.PageSize);
        }
    }
}
