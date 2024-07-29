using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class FilterOptions
    {
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string? Query { get; set; }

    }
}
