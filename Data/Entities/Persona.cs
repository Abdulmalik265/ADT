using Microsoft.AspNetCore.Identity;


namespace Data.Entities
{
    public class Persona : IdentityUser<Guid> ,  IBaseEntity
    {
        public string Surname { get; set; }
        public string FirstName { get; set; }

        public string? OtherNames { get; set; }
        public string FullName { get => $"{FirstName} {Surname} {OtherNames}"; }
        public string? Password { get; set; }
        public Guid CreatedBy { get; set; }
        public virtual DateTime Created { get; protected set; }
        public virtual DateTime? Modified { get; set; }
        public virtual string? LastModifiedBy { get; set; }

        public Persona(DateTime created, bool isDeleted)
        {
            Created = created;
            IsDeleted = isDeleted;
            Id = Guid.NewGuid();
        }
        public Persona() : this(DateTime.UtcNow, false) { }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool IsDeleted { get; set; }
    }
}
