using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class SMSViewModel
    {
        public string? RecipientType { get; set; }
        public Guid? StateId { get; set; }
        public Guid? LocalGovernmentId { get; set; }
        public string Message { get; set; }

    }
}
