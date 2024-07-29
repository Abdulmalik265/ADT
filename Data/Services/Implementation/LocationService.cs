using Core.IRepo;
using Core.Models;
using Data.Services.Interface;
using Microsoft.EntityFrameworkCore;


namespace Data.Services.Implementation
{
    public class LocationService : ILocationService
    {
        private readonly AdtDbContext _context;
        private readonly IMembersRepo _membersRepo;
        private readonly IAdminRepo _adminRepo;
        private readonly IDirectorRepo _directorRepo;
        public LocationService(AdtDbContext context, IMembersRepo membersRepo, IAdminRepo adminRepo, IDirectorRepo directorRepo )
        {
            _context = context;
            _membersRepo = membersRepo;
            _adminRepo = adminRepo;
            _directorRepo = directorRepo;
        }
        public async Task<IEnumerable<StateViewModel>> GetStatesAsync()
        {
            return await _context.States.Select(x => new StateViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).OrderBy(x=> x.Name)  
               .ToListAsync();
        }
        public async Task<IEnumerable<LocalGovernmentViewModel>> GetLocalGovernmentsAsync(Guid stateId)
        {
            return await _context.localGovernments.Where(x => x.StateId == stateId)
                .Select(x => new LocalGovernmentViewModel
                {
                    Id = x.Id,
                    Name= x.Name,
                    StateId = x.StateId

                }).OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAllPhoneNumbersAsync()
        {
            var membersPhone = await _membersRepo.GetAllMembersPhoneNumber();
            var adminsPhone = await _adminRepo.GetAllAdminPhoneNumbers();
            var directorsPhone = await _directorRepo.GetAllDirectorPhoneNumbers();

            return membersPhone.Concat(adminsPhone).Concat(directorsPhone).Distinct();
        }

        public async Task<IEnumerable<string>> GetAllPhoneNumbersAsyncPerState(Guid stateId)
        {
            var membersPhone = await _membersRepo.GetAllMembersPhoneNumberPerState(stateId);
            var adminsPhone = await _adminRepo.GetAllAdminPhoneNumbersPerState(stateId);
            var directorsPhone = await _directorRepo.GetAllDirectorPhoneNumbersPerState(stateId);

            return membersPhone.Concat(adminsPhone).Concat(directorsPhone).Distinct();
        }

        public async Task<IEnumerable<string>> GetAllPhoneNumbersAsync(Guid userId)
        {
            var membersPhone = await _membersRepo.GetAllMembersPhoneNumber(userId);
            var adminsPhone = await _adminRepo.GetAllAdminPhoneNumbers(userId);
            var directorsPhone = await _directorRepo.GetAllDirectorPhoneNumbers(userId);

            return membersPhone.Concat(adminsPhone).Concat(directorsPhone).Distinct();
        }

        public async Task<IEnumerable<string>> GetAllPhoneNumbersAsync(Guid stateId, Guid lgaId)
        {
            var membersPhone = await _membersRepo.GetAllMembersPhoneNumber(stateId, lgaId);
            var adminsPhone = await _adminRepo.GetAllAdminPhoneNumbers(stateId, lgaId);
            var directorsPhone = await _directorRepo.GetAllDirectorPhoneNumbers(stateId);

            return membersPhone.Concat(adminsPhone).Concat(directorsPhone).Distinct();
        }

        public async Task<IEnumerable<string>> GetPhoneNumbersByStateAndLGAAsync(Guid stateId, Guid lgaId, string RecipientType)
        {
            IEnumerable<string> phoneNumbers = new List<string>();
            switch (RecipientType)
            {
                case "Directors":
                    phoneNumbers = await _directorRepo.GetAllDirectorPhoneNumbersPerState(stateId) ;
                    break;
                case "Admins":
                    phoneNumbers = await _adminRepo.GetAllAdminPhoneNumbers(stateId, lgaId);
                    break;
                case "Members":
                    phoneNumbers = await _membersRepo.GetAllMembersPhoneNumber(stateId, lgaId);
                    break;
                case "All":
                    phoneNumbers = await GetAllPhoneNumbersAsync(stateId, lgaId);
                    break;
            }

            return phoneNumbers;
        } 
        
        public async Task<IEnumerable<string>> GetPhoneNumbersByStateAndLGAAsync(Guid stateId, string RecipientType)
        {
            IEnumerable<string> phoneNumbers = new List<string>();
            switch (RecipientType)
            {
                case "Directors":
                    phoneNumbers = await _directorRepo.GetAllDirectorPhoneNumbersPerState(stateId);
                    break;
                case "Admins":
                    phoneNumbers = await _adminRepo.GetAllAdminPhoneNumbersPerState(stateId);
                    break;
                case "Members":
                    phoneNumbers = await _membersRepo.GetAllMembersPhoneNumberPerState(stateId);
                    break;
                case "All":
                    phoneNumbers = await GetAllPhoneNumbersAsyncPerState(stateId);
                    break;
            }

            return phoneNumbers;
        }

         public async Task<DashboardReportViewModel> GetDashboardReportAsync()
            {
                var currentDate = DateTime.UtcNow;
                var currentMonth = currentDate.Month;
                var currentYear = currentDate.Year;
                var previousMonth = currentDate.AddMonths(-1).Month;
                var previousYear = currentDate.AddYears(-1).Year;

                // Initialize the view model
                var report = new DashboardReportViewModel();

                // Get the total payments for the current month
                report.CurrentMonthTotalPayment = await _context.PaymentInfos
                    .Where(p => p.Created.Month == currentMonth && p.Created.Year == currentYear)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                // Get the total payments for the previous month
                report.PreviousMonthTotalPayment = await _context.PaymentInfos
                    .Where(p => p.Created.Month == previousMonth && p.Created.Year == currentYear)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                // Get the total payments for the current year
                report.CurrentYearTotalPayment = await _context.PaymentInfos
                    .Where(p => p.Created.Year == currentYear)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                // Get the total payments for the previous year
                report.PreviousYearTotalPayment = await _context.PaymentInfos
                    .Where(p => p.Created.Year == previousYear)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                // Get the total number of members
                report.NumberOfMembers = await _context.Members.CountAsync();

                // Get the number of states with at least one member
                report.NumberOfStates = await _context.Members
                    .Select(m => m.LocalGovernment.StateId)
                    .Distinct()
                    .CountAsync();

                // Get the number of local governments with at least one member
                report.NumberOfLGA = await _context.Members
                    .Select(m => m.LocalGovernmentId)
                    .Distinct()
                    .CountAsync();

                return  report;
            }

        }
    
}
