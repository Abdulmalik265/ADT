using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Constants
{
    public class StringConstants
    {
        public const string PHONE_NUMBER_REGEX = @"^0[7-9]{1}\d{9}$";

    }

    public static class RoleConstants
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Director = "Director";
        public const string Coordinator = "Coordinator";
    }
}
