using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class DashboardReportViewModel
    {
        public decimal CurrentMonthTotalPayment { get; set; }
        public decimal CurrentYearTotalPayment { get; set; }

        public decimal PreviousMonthTotalPayment { get; set; }
        public decimal PreviousYearTotalPayment { get; set; }

        public int NumberOfMembers { get; set; }
        public int NumberOfStates { get; set; }
        public int NumberOfLGA { get; set; }
    }
}
