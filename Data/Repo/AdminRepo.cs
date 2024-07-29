using Azure.Core;
using Core.Constants;
using Core.IRepo;
using Core.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Data.Repo
{
    public class AdminRepo : IAdminRepo
    {
        private readonly AdtDbContext _context;
        private readonly UserManager<Persona> _userManager;
        private readonly RoleManager<Role> _roleManager;
        public AdminRepo(AdtDbContext context, RoleManager<Role> roleManager, UserManager<Persona> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<BaseResponse> CreateAsync(AdminViewModel request, string createdBy)
        {
            var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == request.UserName || x.PhoneNumber == request.PhoneNumber || x.Email == request.Email);
            if (userInDb != null)
                return new BaseResponse() { Status = false, Message = "Admin with same email or phone number or username exists" };

            var creator = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == createdBy);
            if (creator == null)
                return new BaseResponse() { Status = false, Message = "Creator not found" };

            var creatorDirector = await _context.Directors.SingleOrDefaultAsync(x => x.UserId == creator.Id);
            if (creatorDirector == null)
                return new BaseResponse() { Status = false, Message = "Creator's director record not found" };

            var creatorStateId = creatorDirector.StateId; // Assuming StateId is available in the Director entity

            var localGovernment = await _context.localGovernments.SingleOrDefaultAsync(x => x.Id == request.LocalGovernmentId);
            if (localGovernment == null)
                return new BaseResponse() { Status = false, Message = "Invalid LocalGovernmentId" };

            if (localGovernment.StateId != creatorStateId)
                return new BaseResponse() { Status = false, Message = "Local Government does not belong to the creator's state" };

            Persona newUser = new Persona
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                PhoneNumber = request.PhoneNumber,
                OtherNames = request.OtherName,
                Surname = request.SurName,
                Email = request.Email,
                UserName = request.PhoneNumber,
                LockoutEnabled = false,
                PhoneNumberConfirmed = true,
                Password = "12345678"
            };
            newUser.UserName = request.PhoneNumber;

            Admin admin = new Admin
            {
                Id = Guid.NewGuid(),
                LastModifiedBy = createdBy,
                UserId = newUser.Id,
                Created = DateTime.UtcNow,
                Gender = request.Gender,
                IsDeleted = false,
                LocalGovernmentId = request.LocalGovernmentId
            };

            _context.Admins.Add(admin);
            var status = await _context.TrySaveChangesAsync();

            await _userManager.SetUserNameAsync(newUser, request.PhoneNumber);
            if (!string.IsNullOrEmpty(request.Email))
                await _userManager.SetEmailAsync(newUser, request.Email);

            var result = await _userManager.CreateAsync(newUser, newUser.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, RoleConstants.Coordinator);
            }

            if (status)
                return new BaseResponse() { Status = true, Message = "Admin created successfully!" };

            return new BaseResponse() { Status = false, Message = "Unable to create Admin!" };
        }

        public async Task<BaseResponse> DeleteAsync(Guid id, string updatedBy)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the Admin
                    var admin = _context.Admins.SingleOrDefault(x => x.Id == id);
                    if (admin == null)
                    {
                        return new BaseResponse() { Status = false, Message = "Admin not found!" };
                    }

                    // Fetch the user
                    var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == admin.UserId);
                    if (userInDb == null)
                    {
                        return new BaseResponse() { Status = false, Message = "User not found!" };
                    }

                    // Remove the entities
                    _context.Admins.Remove(admin);
                    var userDeleteResult = await _userManager.DeleteAsync(userInDb);
                    if (!userDeleteResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return new BaseResponse() { Status = false, Message = "Failed to delete user!" };
                    }

                    // Save changes
                    var status = await _context.TrySaveChangesAsync();
                    if (!status)
                    {
                        await transaction.RollbackAsync();
                        return new BaseResponse() { Status = false, Message = "Sorry, we had a server error!" };
                    }

                    await transaction.CommitAsync();
                    return new BaseResponse() { Status = true, Message = "Admin deleted successfully!" };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return new BaseResponse() { Status = false, Message = "An error occurred while deleting the admin!" };
                }
            }

        }

        public Task<IEnumerable<AdminViewModel>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetAllAdminPhoneNumbers()
        {
            var adminPhoneNumbers = await _context.Admins
                .Select(Admin => _userManager.Users
                .Where(user => user.Id == Admin.UserId)
                .Select(user => user.PhoneNumber)
                .ToList()
                )
                .ToListAsync();

            var allPhoneNumbers = adminPhoneNumbers.SelectMany(phoneNumbers => phoneNumbers);

            return allPhoneNumbers;
        }
        public async Task<AdminViewModel?> GetByIdAsync(Guid id)
        {
            var adminInDB = await _context.Admins
                .Include(x => x.LocalGovernment).ThenInclude(x => x.State)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (adminInDB == null)
                return null;

            var UserInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == adminInDB.UserId);
            if (UserInDb == null)
                return null;

            var roleInDb = await _userManager.GetRolesAsync(UserInDb);
            var admin = new AdminViewModel
            {
                Id = adminInDB.Id,
                FirstName = UserInDb.FirstName,
                SurName = UserInDb.Surname,
                OtherName = UserInDb.OtherNames,
                PhoneNumber = UserInDb.PhoneNumber,
                Email = UserInDb.Email,
                UserName = UserInDb.UserName,
                Role = roleInDb.ToString(),
                Gender = adminInDB.Gender,
                LocalGovernmentId = adminInDB.LocalGovernmentId,
                LocalGovernment = adminInDB.LocalGovernment is null ? new LocalGovernmentViewModel() : new LocalGovernmentViewModel
                {
                    Name = adminInDB.LocalGovernment.Name,
                    StateId = adminInDB.LocalGovernment.StateId,
                    State = new StateViewModel
                    {
                        Id = adminInDB.LocalGovernment.StateId,
                        Name = adminInDB.LocalGovernment.State.Name
                    }

                }


            };
            return admin;
        }

        public async Task<AdminViewModel> GetByUserIdAsync(Guid id)
        {
            var adminInDb = await _context.Admins.Include(x => x.LocalGovernment)
                .ThenInclude(x => x.State)
                .SingleOrDefaultAsync(x => x.UserId == id);
            var userInDb = await _userManager.Users.SingleAsync(x => x.Id == adminInDb!.UserId);
            var roleInDb = await _userManager.GetRolesAsync(userInDb);
            AdminViewModel admin = new AdminViewModel
            {
                Id = adminInDb.Id,
                Gender = adminInDb.Gender,
                LocalGovernmentId = adminInDb.LocalGovernmentId,
                UserId = adminInDb.UserId,
                LocalGovernment = new LocalGovernmentViewModel
                {
                    Id = adminInDb.LocalGovernmentId,
                    Name = adminInDb.LocalGovernment.Name,
                    State = new StateViewModel
                    {
                        Id = adminInDb.LocalGovernment.StateId,
                        Name = adminInDb.LocalGovernment.State.Name
                    }

                },
                Email = userInDb.Email,
                FirstName = userInDb.FirstName,
                OtherName = userInDb.OtherNames,
                SurName = userInDb.Surname,
                PhoneNumber = userInDb.PhoneNumber,
                UserName = userInDb.UserName,
                Password = userInDb.Password,
                Role = roleInDb.ToString()

            };


            return admin;
        }

        public async Task<PaginatedList<AdminViewModel>> GetPaginatedListAsync(FilterOptions options)
        {
            var adminsInDb = _context.Admins.Include(x => x.LocalGovernment)
                .Select(x => new AdminViewModel
                {

                    Id = x.Id,
                    UserId = x.UserId,
                    State = x.LocalGovernment.State.Name,
                    Lga = x.LocalGovernment.Name

                }).AsNoTracking();


            var count = await adminsInDb.CountAsync();
            var items = await adminsInDb
                        .Skip((options.PageIndex - 1) * options.PageSize)
                        .Take(options.PageSize)
                        .ToListAsync();

            foreach (var item in items)
            {
                var user = await _userManager.FindByIdAsync(item.UserId.ToString());
                if (user != null)
                {
                    item.PhoneNumber = user.PhoneNumber;
                    item.SurName = user.Surname;
                    item.OtherName = user.OtherNames;
                    item.FirstName = user.FirstName;
                }
            }


            return PaginatedList<AdminViewModel>.Create(items, count, options);
        }

        public async Task<BaseResponse> UpdateAsync(AdminViewModel request, string updatedBy)
        {
            var admin = _context.Admins.SingleOrDefault(x => x.Id == request.Id);
            if (admin == null)
                return new BaseResponse { Status = false, Message = "No Admin Found" };
            var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == admin.UserId);
            if (userInDb == null)
                return new BaseResponse();
            var roleInDb = await _userManager.GetRolesAsync(userInDb);

            userInDb.FirstName = request.FirstName;
            admin.LocalGovernmentId = request.LocalGovernmentId;
            userInDb.PhoneNumber = request.PhoneNumber;
            userInDb.Surname = request.SurName;
            userInDb.Email = request.Email;


            var userChanges = await _userManager.UpdateAsync(userInDb);
            await _context.SaveChangesAsync();

            _context.Update(admin);

            var status = await _context.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "Admin updated successfully!" };
            return new BaseResponse() { Status = false, Message = "Unable to update Admin!" };
        }

        public async Task<BaseResponse> UpdateProfile(string UserName, string Email, string Password, Guid id)
        {
            var adminInD = await _context.Admins.SingleOrDefaultAsync(x => x.Id == id);
            var checker = await _userManager.Users.AnyAsync(x => x.UserName == UserName && x.Email == Email);
            if (checker)
                return new BaseResponse() { Status = false, Message = "User with same username or email exist!" };
            var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == adminInD!.UserId);

            userInDb.UserName = UserName;
            userInDb.Email = Email;
            userInDb.Password = Password;

            await _userManager.UpdateAsync(userInDb);
            var status = await _context.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "You successfully update your profile" };
            return new BaseResponse() { Status = false, Message = "Unable to update profile!" };
        }

        public async Task<PaginatedList<AdminViewModel>> GetPaginatedListAsync(FilterOptions options, Guid userId)
        {
            var userInDb = await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
            if (userInDb == null)
                return null;
            var directorInDb = await _context.Directors.SingleOrDefaultAsync(x => x.UserId == userInDb.Id);
            if (directorInDb == null)
                return null;

            var directorStateId = directorInDb.StateId;

            var adminsInDb = _context.Admins
                .Include(x => x.LocalGovernment).ThenInclude(x=> x.State)
                .Where(x => x.LocalGovernment.State.Id == directorStateId)
                .Select(x => new AdminViewModel
                {

                    Id = x.Id,
                    UserId = x.UserId,
                    State = x.LocalGovernment.State.Name,
                    Lga = x.LocalGovernment.Name

                }).AsNoTracking();


            var count = await adminsInDb.CountAsync();
            var items = await adminsInDb
                        .Skip((options.PageIndex - 1) * options.PageSize)
                        .Take(options.PageSize)
                        .ToListAsync();

            foreach (var item in items)
            {
                var user = await _userManager.FindByIdAsync(item.UserId.ToString());
                if (user != null)
                {
                    item.PhoneNumber = user.PhoneNumber;
                    item.SurName = user.Surname;
                    item.OtherName = user.OtherNames;
                    item.FirstName = user.FirstName;
                }
            }


            return PaginatedList<AdminViewModel>.Create(items, count, options);
        }

        public async Task<IEnumerable<string>> GetAllAdminPhoneNumbers(Guid userId)
        {
            var adminInDb = await _context.Admins.SingleOrDefaultAsync(x => x.UserId == userId);
            var adminPhoneNumbers = await _context.Admins
                           .Where(x => x.LocalGovernment.StateId == adminInDb.LocalGovernment.StateId)
                          .Select(Admin => _userManager.Users
                          .Where(user => user.Id == Admin.UserId)
                          .Select(user => user.PhoneNumber)
                                        .ToList()

                          )
                          .ToListAsync();

            var allPhoneNumbers = adminPhoneNumbers.SelectMany(phoneNumbers => phoneNumbers);

            return allPhoneNumbers;
        }

        public async Task<IEnumerable<string>> GetAllAdminPhoneNumbers(Guid stateId, Guid lgaId)
        {
            var adminPhoneNumbers = await _context.Admins
              .Where(x=>x.LocalGovernment.StateId == stateId && x.LocalGovernmentId == lgaId)
              .Select(Admin => _userManager.Users
              .Where(user => user.Id == Admin.UserId)
              .Select(user => user.PhoneNumber)
              .ToList()

              )
              .ToListAsync();

            var allPhoneNumbers = adminPhoneNumbers.SelectMany(phoneNumbers => phoneNumbers);

            return allPhoneNumbers;
        }

        public async Task<IEnumerable<string>> GetAllAdminPhoneNumbersPerState(Guid stateId)
        {
            var adminPhoneNumbers = await _context.Admins
                .Where(x=> x.LocalGovernment.StateId == stateId)
               .Select(Admin => _userManager.Users
               .Where(user => user.Id == Admin.UserId)
               .Select(user => user.PhoneNumber)
                 .ToList()

               )
               .ToListAsync();

            var allPhoneNumbers = adminPhoneNumbers.SelectMany(phoneNumbers => phoneNumbers);

            return allPhoneNumbers;
        }

        public async Task<IEnumerable<AdminsPaymentViewModel>> filterAdminsPayment(AttendenceReportViewModel model)
        {
            var adminsInDb = await _context.Admins
                .Include(x => x.LocalGovernment)
                .ThenInclude(x => x.State)
                .Where(x => x.LocalGovernment.StateId == model.StateId)
                .ToListAsync();

            var LgaPaymentInfo = new List<AdminsPaymentViewModel>();
            var totalAmount = 0m;

            foreach (var admin in adminsInDb)
            {
                var persona = await _userManager.FindByIdAsync(admin.UserId.ToString());

                var members = await _context.Members
                    .Where(x => x.LocalGovernmentId == admin.LocalGovernmentId)
                    .ToListAsync();

                var payments = await _context.PaymentInfos
                    .Where(x => members.Select(m => m.Id).Contains(x.MemberId) &&
                        x.Month == model.Month && x.Created.Year == model.Year)
                    .ToListAsync();


                totalAmount = payments.Sum(x => x.Amount); // Reset and calculate totalAmount for each admin

                var existingLga = LgaPaymentInfo.FirstOrDefault(x => x.LgaName == admin.LocalGovernment.Name);
                if (existingLga == null)
                {
                    existingLga = new AdminsPaymentViewModel
                    {
                        LgaName = admin.LocalGovernment.Name,
                        Admins = new List<AdminsInfo>()
                    };
                    LgaPaymentInfo.Add(existingLga); // Add the new LGA to the list
                }

                existingLga.Admins.Add(new AdminsInfo
                {
                    AdminName = persona.FullName,
                    AdminPhoneNumber = persona.PhoneNumber,
                    TotalAmount = totalAmount,
                });
            }

            return LgaPaymentInfo;
        }
    }
}
