using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class LocalGovernment : Base
    {
        public string Name { get; set; }
        public Guid StateId { get; set; }
        public State State { get; set; }
    }
}
