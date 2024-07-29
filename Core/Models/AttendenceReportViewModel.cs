using Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class AttendenceReportViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? StateId { get; set; }
        public Guid? LgaId { get; set; }
        public Month? Month { get; set; }
        public int? Year { get; set; }

    }
}
