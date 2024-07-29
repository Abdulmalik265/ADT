using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Base : ICloneable
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? Modified { get; set; }
        public string? LastModifiedBy { get; set; }

        protected Base()
        {
            IsDeleted = false;
            Created = DateTime.UtcNow;
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
