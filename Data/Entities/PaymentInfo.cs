using Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class PaymentInfo : Base
    {
        public bool IsPaid { get; set; } = false;
        public decimal Amount { get; set; }
        public Month Month { get; set; }
        public Guid MemberId { get; set; }
        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }
    }
}
