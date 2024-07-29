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
    public class DirectorViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "First Name is required!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Surname is required!")]

        public string SurName { get; set; }
        public string? OtherName { get; set; }
        public string FullName => $"{FirstName} {SurName}";
        [Required(ErrorMessage = "Phone Number is required!")]
        [RegularExpression(StringConstants.PHONE_NUMBER_REGEX, ErrorMessage = "Invalid phone number")]

        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public Gender Gender { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public Guid UserId { get; set; }
        public Guid StateId { get; set; }
        public StateViewModel State { get; set; }
    }
}
