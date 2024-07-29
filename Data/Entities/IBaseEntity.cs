using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public interface IBaseEntity : ICloneable
    {
        public bool IsDeleted { get; protected set; }
         

    }
}
