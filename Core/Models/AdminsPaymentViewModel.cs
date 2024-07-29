using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class AdminsPaymentViewModel
    {
        public string LgaName { get; set; }
        public List<AdminsInfo> Admins { get; set; }
    }
    public class AdminsInfo
    {
        public string AdminName { get; set; }
        public string AdminPhoneNumber { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
