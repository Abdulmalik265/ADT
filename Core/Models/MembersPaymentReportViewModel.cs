using Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class MembersPaymentReportViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string FullName => $"{FirstName} {SurName}";
        public string PhoneNumber { get; set; }
        public Month Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public string LgName { get; set; }
        public string? StateName { get; set; }
    }
}
