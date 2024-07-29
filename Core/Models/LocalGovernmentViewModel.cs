using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class LocalGovernmentViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid StateId { get; set; }

        public StateViewModel State { get; set; }
    }
}
