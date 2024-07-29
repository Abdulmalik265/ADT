using Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Director : Base
    {
        public Guid UserId { get; set; }
        public Gender Gender { get; set; }
        public Guid StateId { get; set; }
        [ForeignKey(nameof(StateId))]
        public State State { get; set; }

    }
}
