using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Role : IdentityRole<Guid>, IBaseEntity
    {
        public object Clone()
        {
            throw new NotImplementedException();
        }

        public bool IsDeleted { get; set; }
    }
}
