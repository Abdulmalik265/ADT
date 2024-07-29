using Core.Constants;
using Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class MembersViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string FullName => $"{FirstName} {SurName}";

        public string? OtherNames { get; set; }
        [Required(ErrorMessage = "Phone Number is required!")]
        [RegularExpression(StringConstants.PHONE_NUMBER_REGEX, ErrorMessage = "Invalid phone number")]

        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public Gender Gender { get; set; }
        public Guid LocalGovernmentId { get; set; }
        public LocalGovernmentViewModel LocalGovernment { get; set; }
        public Qualification Qualification { get; set; }
        public string Age { get; set; }
        public double? Amount { get; set; }
        public Month? Month { get; set; }
    }
   
}

