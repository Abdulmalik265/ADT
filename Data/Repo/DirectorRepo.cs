using Core.Constants;
using Core.IRepo;
using Core.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Data.Repo
{
    public class DirectorRepo : IDirectorRepo
    {
        private readonly AdtDbContext _context;
        private readonly UserManager<Persona> _userManager;
        private readonly RoleManager<Role> _roleManager;
       
        public DirectorRepo(AdtDbContext context, RoleManager<Role> roleManager, UserManager<Persona> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
           

        }
        public async Task<BaseResponse> CreateAsync(DirectorViewModel request, string createdBy)
        {
            var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == request.UserName || x.PhoneNumber == request.PhoneNumber || x.Email == request.Email);
            if (userInDb != null)
                return new BaseResponse() { Status = false, Message = "Director with same email or phone number or username exist" };

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
                Password = "Aa12345678"
            };
            newUser.UserName = request.PhoneNumber;
            Director director = new Director
            {
                Id = Guid.NewGuid(),
                LastModifiedBy = createdBy,
                UserId = newUser.Id,
                Created = DateTime.UtcNow,
                Gender = request.Gender,
                IsDeleted = false,
                StateId = request.StateId
            };
           


            _context.Directors.Add(director);
            var status = await _context.TrySaveChangesAsync();


            await _userManager.SetUserNameAsync(newUser, request.PhoneNumber);
            if (!string.IsNullOrEmpty(request.Email))
                await _userManager.SetEmailAsync(newUser, request.Email);

            var result = await _userManager.CreateAsync(newUser, newUser.Password);

            if (result.Succeeded)
            {
                var results = await _userManager.AddToRoleAsync(newUser, RoleConstants.Director);
            }
            if (status)
                return new BaseResponse() { Status = true, Message = "Director created successfully!" };

            return new BaseResponse() { Status = false, Message = "Unable to create director check back!" };

        }

        public async Task<BaseResponse> DeleteAsync(Guid id, string updatedBy)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the director
                    var director = _context.Directors.SingleOrDefault(x => x.Id == id);
                    if (director == null)
                    {
                        return new BaseResponse() { Status = false, Message = "Director not found!" };
                    }

                    // Fetch the user
                    var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == director.UserId);
                    if (userInDb == null)
                    {
                        return new BaseResponse() { Status = false, Message = "User not found!" };
                    }

                    // Remove the entities
                    _context.Directors.Remove(director);
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
                    return new BaseResponse() { Status = true, Message = "Director deleted successfully!" };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return new BaseResponse() { Status = false, Message = "An error occurred while deleting the director!" };
                }
            }

        }

        public async Task<IEnumerable<DirectorsPaymentReportViewModel>> FilterDirectorsPayment(AttendenceReportViewModel model)
        {
            // Retrieve all directors
            var directors = await _context.Directors
                .Include(d => d.State)
                .ToListAsync();

            var statePaymentInfo = new List<DirectorsPaymentReportViewModel>();

            foreach (var director in directors)
            {
                // Retrieve persona information using UserManager
                var persona = await _userManager.FindByIdAsync(director.UserId.ToString());

                // Retrieve all admins in the state of the director
                var admins = await _context.Admins
                    .Where(a => a.LocalGovernment.StateId == director.StateId)
                    .Include(a => a.LocalGovernment)
                    .ToListAsync();

                var totalAmount = 0m;

                foreach (var admin in admins)
                {
                    // Retrieve all members created by the admin
                    var members = await _context.Members
                        .Where(m => m.LocalGovernmentId == admin.LocalGovernmentId)
                        .ToListAsync();

                    // Retrieve payment information for these members
                    var payments = await _context.PaymentInfos
                        .Where(p => members.Select(m => m.Id).Contains(p.MemberId) &&
                                    p.Month == model.Month && p.Created.Year == model.Year)
                        .ToListAsync();

                    totalAmount += payments.Sum(p => p.Amount);
                }

                var existingState = statePaymentInfo.FirstOrDefault(sp => sp.StateName == director.State.Name);
                if (existingState == null)
                {
                    existingState = new DirectorsPaymentReportViewModel
                    {
                        StateName = director.State.Name,
                        Directors = new List<DirectorInfo>()
                    };
                    statePaymentInfo.Add(existingState);
                }

                existingState.Directors.Add(new DirectorInfo
                {
                    DirectorName = persona.FullName,
                    DirectorPhoneNumber = persona.PhoneNumber,
                    TotalAmount = totalAmount
                });
            }

            return statePaymentInfo;
        }


        public Task<IEnumerable<DirectorViewModel>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetAllDirectorPhoneNumbers()
        {
            var directorPhoneNumbers = await _context.Directors
               .Select(Director => _userManager.Users
               .Where(user => user.Id == Director.UserId)
               .Select(user => user.PhoneNumber)
                             .ToList()

               )
               .ToListAsync();

            var allPhoneNumbers = directorPhoneNumbers.SelectMany(phoneNumbers => phoneNumbers);

            return allPhoneNumbers;
        }
        public async Task<IEnumerable<string>> GetAllDirectorPhoneNumbers(Guid userId)
        {
            var directorINDb = await _context.Directors.SingleOrDefaultAsync(x=> x.UserId == userId);
            
            var directorPhoneNumbers = await _context.Directors
                .Where(x =>x.StateId == directorINDb.StateId)
               .Select(Director => _userManager.Users
               .Where(user => user.Id == Director.UserId)

               .Select(user => user.PhoneNumber)
                             .ToList()

               )
               .ToListAsync();

            var allPhoneNumbers = directorPhoneNumbers.SelectMany(phoneNumbers => phoneNumbers);

            return allPhoneNumbers;
        }

        public async Task<IEnumerable<string>> GetAllDirectorPhoneNumbersPerState(Guid stateId)
        {
            var directorPhoneNumbers = await _context.Directors
              .Where(x=> x.StateId == stateId)
             .Select(Director => _userManager.Users
             .Where(user => user.Id == Director.UserId)
             .Select(user => user.PhoneNumber)
                           .ToList()

             )
             .ToListAsync();

            var allPhoneNumbers = directorPhoneNumbers.SelectMany(phoneNumbers => phoneNumbers);

            return allPhoneNumbers;
        }


        public async Task<DirectorViewModel?> GetByIdAsync(Guid id)
        {
            var directorInDB = await _context.Directors
                .Include(x => x.State).SingleOrDefaultAsync(x => x.Id == id);
            if (directorInDB == null)
                return null;

            var UserInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == directorInDB.UserId);
            if (UserInDb == null)
                return null;
            var roleInDb = await _userManager.GetRolesAsync(UserInDb);
            var admin = new DirectorViewModel
            {
                Id = directorInDB.Id,
                FirstName = UserInDb.FirstName,
                SurName = UserInDb.Surname,
                OtherName = UserInDb.OtherNames,
                PhoneNumber = UserInDb.PhoneNumber,
                Email = UserInDb.Email,
                UserName = UserInDb.UserName,
                Role = roleInDb.ToString(),
                Gender = directorInDB.Gender,
                StateId = directorInDB.StateId,
                State = new StateViewModel
                {
                    Name = directorInDB.State.Name,
                    Id = directorInDB.State.Id
                    
                }


            };
            return admin;
        }

        public async Task<DirectorViewModel> GetByUserIdAsync(Guid id)
        {
            var directorInDb = await _context.Directors.Include(x=> x.State)
                .SingleOrDefaultAsync(x => x.UserId == id);
            var userInDb = await _userManager.Users.SingleAsync(x => x.Id == directorInDb!.UserId);
            var roleInDb = await _userManager.GetRolesAsync(userInDb);
            DirectorViewModel director = new DirectorViewModel
            {
                Id = directorInDb.Id,
                Gender = directorInDb.Gender,
                StateId = directorInDb.StateId,
                UserId = directorInDb.UserId,
                State = new StateViewModel
                {
                    Id = directorInDb.StateId,
                    Name = directorInDb.State.Name
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


            return director;
        }

        public async Task<PaginatedList<DirectorViewModel>> GetPaginatedListAsync(FilterOptions options)
        {
            var directorsInDb = _context.Directors.Include(x => x.State)
                .Select(x => new DirectorViewModel
                {

                    Id = x.Id,
                    UserId = x.UserId,
                    State = new StateViewModel
                    {
                        Name = x.State.Name,
                        Id = x.State.Id
                    }

                }).AsNoTracking();


            var count = await directorsInDb.CountAsync();
            var items = await directorsInDb
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


            return PaginatedList<DirectorViewModel>.Create(items, count, options);
        }

        public async Task<BaseResponse> UpdateAsync(DirectorViewModel request, string updatedBy)
        {
            var director = _context.Directors.SingleOrDefault(x => x.Id == request.Id);
            if (director == null)
                return new BaseResponse { Status = false, Message = "No Director Found" };
            var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == director.UserId);
            if (userInDb == null)
                return new BaseResponse();
            var roleInDb = await _userManager.GetRolesAsync(userInDb);

            userInDb.FirstName = request.FirstName;
            director.StateId = request.StateId;
            userInDb.PhoneNumber = request.PhoneNumber;
            userInDb.Surname = request.SurName;
            userInDb.Email = request.Email;


            var userChanges = await _userManager.UpdateAsync(userInDb);
            await _context.SaveChangesAsync();

            _context.Update(director);

            var status = await _context.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "Director updated successfully!"};
            return new BaseResponse() { Status = false, Message = "Unable to update Admin!" };
        }

        public async Task<BaseResponse> UpdateProfile(string UserName, string Email, string Password, Guid id)
        {
            var directorInD = await _context.Directors.SingleOrDefaultAsync(x => x.Id == id);
            var checker = await _userManager.Users.AnyAsync(x => x.UserName == UserName && x.Email == Email);
            if (checker)
                return new BaseResponse() { Status = false, Message = "User with same username or email exist!" };
            var userInDb = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == directorInD!.UserId);

            userInDb.UserName = UserName;
            userInDb.Email = Email;
            userInDb.Password = Password;

            await _userManager.UpdateAsync(userInDb);
            var status = await _context.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "You successfully update your profile" };
            return new BaseResponse() { Status = false, Message = "Unable to update profile!" };
        }
    }
}

