using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class DirectorsPaymentReportViewModel
    {
        public string StateName { get; set; }
        public List<DirectorInfo> Directors { get; set; }
    }

    public class DirectorInfo
    {
        public string DirectorName { get; set; }
        public string DirectorPhoneNumber { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
