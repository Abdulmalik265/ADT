using Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class PaymentInfoViewModel
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public Month Month { get; set; }
        public Guid MemberId { get; set; }
        public MembersViewModel Member { get; set; }
    }
}
