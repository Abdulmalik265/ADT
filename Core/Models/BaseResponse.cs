using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class BaseResponse
    {
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } = true;
    }
}
