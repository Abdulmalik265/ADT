using Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Member : Base
    {
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string? OtherNames { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public Gender Gender { get; set; }
        public string Age { get; set; }
        public Qualification Qualification { get; set; }
        public Guid LocalGovernmentId { get; set; }
        [ForeignKey(nameof(LocalGovernmentId))]
        public LocalGovernment LocalGovernment { get; set; }


    }
}
