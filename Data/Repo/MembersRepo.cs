using Core.IRepo;
using Core.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Claims;


namespace Data.Repo
{
    public class MembersRepo : IMembersRepo
    {
        private readonly AdtDbContext _context;
        private readonly UserManager<Persona> _userManager;
        public MembersRepo(AdtDbContext context, UserManager<Persona> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<BaseResponse> CreateAsync(MembersViewModel request, string createdBy)
        {
            var check = await _context.Members.SingleOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber || x.Email == request.Email);
            if (check != null)
                return new BaseResponse() { Status = false, Message = "Member with same Phone number or Email found!"};

            var creator = await _context.Users.SingleOrDefaultAsync(x => x.UserName == createdBy);
            if (creator == null)
                return new BaseResponse();
            var creator1 = await _context.Admins.SingleOrDefaultAsync(x => x.UserId == creator.Id);
            if (creator1 == null)
                return new BaseResponse();
            if (request.LocalGovernmentId != creator1.LocalGovernmentId)
                return new BaseResponse() { Status = false, Message = "You can not create a member outside your local government" };
       
            
            Member member = new Member
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                Email = request.Email,
                Gender = request.Gender,
                LocalGovernmentId = request.LocalGovernmentId,
                OtherNames = request.OtherNames,
                PhoneNumber = request.PhoneNumber,
                SurName = request.SurName,
                Qualification = request.Qualification,
                Age = request.Age,
                Created = DateTime.Now.Date
                
            };

            _context.Members.Add(member);
            var status = await _context.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "Member added successfully!"};

            return new BaseResponse() { Status = false, Message = "Server error!" };
        }

        public async Task<BaseResponse> DeleteAsync(Guid id, string updatedBy)
        {
            var member = await _context.Members.SingleOrDefaultAsync(x => x.Id == id);
            if (member == null)
                return new BaseResponse() { Status = false, Message = "Member not fond!" };

            _context.Remove(member);
            var status = await _context.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "Member deleted successfully! " };

            return new BaseResponse() { Status = false, Message = "Server error!" };
        }

        public async Task<IEnumerable<MembersViewModel>> filterMembers(AttendenceReportViewModel model)
        {
            var members = _context.Members.Select(x => new MembersViewModel
            {
                Id = x.Id,
                FirstName = x.FirstName,
                SurName = x.SurName,
                Gender = x.Gender,
                PhoneNumber = x.PhoneNumber,
                LocalGovernmentId = x.LocalGovernmentId,
                LocalGovernment = new LocalGovernmentViewModel
                {
                    Id = x.LocalGovernment.Id,
                    Name = x.LocalGovernment.Name,
                    StateId = x.LocalGovernment.StateId,
                    State = new StateViewModel
                    {
                        Id = x.LocalGovernment.State.Id,
                        Name = x.LocalGovernment.State.Name
                    }
                }

            });


            if (model.StateId != null && model.LgaId != null)
                members = members.Where(x => x.LocalGovernment.StateId == model.StateId && x.LocalGovernmentId == model.LgaId);
            if (model.StateId != null && model.LgaId == null)
                members = members.Where(x => x.LocalGovernment.StateId == model.StateId);
            if (model.LgaId != null)
                members = members.Where(x => x.LocalGovernmentId == model.LgaId);


            return await members.ToListAsync();
        }

        public async Task<IEnumerable<MembersPaymentReportViewModel>> filterMembersPayment(AttendenceReportViewModel model)
        {
            var paymentInfos = await _context.PaymentInfos
                .Include(x=> x.Member)
                .ThenInclude(x=> x.LocalGovernment)
                .Where(x=> x.Month == model.Month && x.Created.Year == model.Year && x.Member.LocalGovernmentId == model.LgaId)
                .Select(x=> new MembersPaymentReportViewModel
                {
                    Id = x.Id,
                    Amount = x.Amount,  
                    FirstName = x.Member.FirstName,
                    SurName = x.Member.SurName,
                    LgName = x.Member.LocalGovernment.Name,
                    Month = x.Month,
                    Year = x.Created.Year,
                    PhoneNumber = x.Member.PhoneNumber
                    
                }).ToListAsync();

            return paymentInfos;
        }

        public Task<IEnumerable<MembersViewModel>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetAllMembersPhoneNumber(Guid userId)
        {
            var adminInDb = await _context.Admins.SingleOrDefaultAsync(x=> x.UserId == userId); 
            
            var allMembersPhoneNumber = await _context.Members.Where(x=> x.LocalGovernmentId == adminInDb.LocalGovernmentId)
                .Select(x=> x.PhoneNumber)
                .ToListAsync();

            return allMembersPhoneNumber;
        }
        public async Task<IEnumerable<string>> GetAllMembersPhoneNumber()
        {

            var allMembersPhoneNumber = await _context.Members
                .Select(x => x.PhoneNumber)

                .ToListAsync();

            return allMembersPhoneNumber;
        }

        public async Task<IEnumerable<string>> GetAllMembersPhoneNumber(Guid stateId, Guid lgaId)
        {
            var allMembersPhoneNumber = await _context.Members
                .Where(x=>x.LocalGovernment.StateId == stateId && x.LocalGovernmentId==lgaId)
                .Select(x => x.PhoneNumber)
                .ToListAsync();

            return allMembersPhoneNumber;
        }

        public async Task<IEnumerable<string>> GetAllMembersPhoneNumberPerState(Guid stateId)
        {
            var allMembersPhoneNumber = await _context.Members
                .Where(x=>x.LocalGovernment.StateId == stateId)
               .Select(x => x.PhoneNumber)
               .ToListAsync();

            return allMembersPhoneNumber;
        }

      

        public async Task<MembersViewModel?> GetByIdAsync(Guid id)
        {
            var member = await _context.Members.Include(x=> x.LocalGovernment).ThenInclude(x=> x.State)
                .Select(x => new MembersViewModel
            {
                Id = x.Id,
                Email = x.Email,
                FirstName = x.FirstName,
                SurName = x.SurName,
                OtherNames = x.OtherNames,
                Gender = x.Gender,
                LocalGovernmentId = x.LocalGovernmentId,
                PhoneNumber = x.PhoneNumber,
                LocalGovernment = new LocalGovernmentViewModel
                {
                    Id = x.LocalGovernmentId,
                    Name = x.LocalGovernment.Name,
                    State = new StateViewModel
                    {
                        Id = x.LocalGovernment.StateId,
                        Name = x.LocalGovernment.State.Name
                    }
                }
                

            }).SingleOrDefaultAsync(x => x.Id == id);

            return member;
        }

        public async Task<PaginatedList<MembersViewModel>> GetPaginatedListAsync(FilterOptions options)
        {
            var member = _context.Members.Include(x=> x.LocalGovernment).ThenInclude(x => x.State)
                .Select(x => new MembersViewModel
            {
                Id = x.Id,
                FirstName = x.FirstName,
                SurName = x.SurName,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                Gender = x.Gender,
                LocalGovernment = new LocalGovernmentViewModel
                {
                    Name = x.LocalGovernment.Name,
                    Id = x.LocalGovernment.Id,
                    StateId = x.LocalGovernment.StateId,
                    State =  new StateViewModel
                    {
                        Name = x.LocalGovernment.State.Name,
                        Id = x.LocalGovernment.StateId
                    }


                }



            });

            var count = await member.CountAsync();
            var items = await member
                        .Skip((options.PageIndex - 1) * options.PageSize)
                        .Take(options.PageSize)
                        .ToListAsync();

            return PaginatedList<MembersViewModel>.Create(items, count, options);
        }

        public async Task<PaginatedList<MembersViewModel>> GetPaginatedListAsync(FilterOptions options, Guid userId)
        {
            var userInDb = await _context.Users.SingleOrDefaultAsync(x=> x.Id ==  userId);
            if (userInDb == null)
                return null;
            var adminInDb = await _context.Admins.SingleOrDefaultAsync(x => x.UserId == userInDb.Id);
            if (adminInDb == null)
                return null;


            var member = _context.Members.Include(x => x.LocalGovernment)
                .ThenInclude(x => x.State).Where(x => x.LocalGovernmentId == adminInDb.LocalGovernmentId)
               .Select(x => new MembersViewModel
               {
                   Id = x.Id,
                   FirstName = x.FirstName,
                   SurName = x.SurName,
                   PhoneNumber = x.PhoneNumber,
                   Email = x.Email,
                   Gender = x.Gender,
                   LocalGovernment = new LocalGovernmentViewModel
                   {
                       Name = x.LocalGovernment.Name,
                       Id = x.LocalGovernment.Id,
                       StateId = x.LocalGovernment.StateId,
                       State = new StateViewModel
                       {
                           Name = x.LocalGovernment.State.Name,
                           Id = x.LocalGovernment.StateId
                       }


                   }



               });

            var count = await member.CountAsync();
            var items = await member
                        .Skip((options.PageIndex - 1) * options.PageSize)
                        .Take(options.PageSize)
                        .ToListAsync();

            return PaginatedList<MembersViewModel>.Create(items, count, options);
        }

        public async Task<BaseResponse> UpdateAsync(MembersViewModel request, string updatedBy)
        {
            var member = await _context.Members.SingleOrDefaultAsync(x => x.Id == request.Id);
            if (member == null)
                return new BaseResponse() { Status = false, Message = "No member found!" };

            member.SurName = request.SurName;
            member.OtherNames = request.OtherNames;
            member.Gender = request.Gender;
            member.PhoneNumber = request.PhoneNumber;
            member.FirstName = request.FirstName;
            member.Qualification = request.Qualification;
            member.PhoneNumber = request.PhoneNumber;
            member.Email = request.Email;
            member.Age = request.Age;
            member.LocalGovernmentId = request.LocalGovernmentId;


            _context.Members.Update(member);
            var status = await _context.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "Member added successfully! " };

            return new BaseResponse() { Status = false, Message = "Server error1" };
        }

        public async Task<BaseResponse> UpdatePaymentInfoAsync(PaymentInfoViewModel model, Guid id)
        {
            var member = await _context.Members.SingleOrDefaultAsync(m => m.Id == id);
            if (member == null)
                return new BaseResponse { Status = true, Message = "Member is not Found!" };

            PaymentInfo paymentInfo = new PaymentInfo
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                Amount = model.Amount,
                IsPaid = true,
                Month = model.Month,
                Created = DateTime.Now
            };
            _context.PaymentInfos.Add(paymentInfo);
            var status = await _context.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "Payment updated successfully!" };

            return new BaseResponse() { Status = false, Message = "Server error!" };
        }
    }
}
