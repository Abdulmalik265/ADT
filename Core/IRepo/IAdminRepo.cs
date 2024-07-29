using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IRepo
{
    public interface IAdminRepo : IBaseRepo<AdminViewModel>
    {
        public Task<AdminViewModel> GetByUserIdAsync(Guid id);
        public Task<BaseResponse> UpdateProfile(string UserName, string Email, string Password, Guid id);
        public Task<IEnumerable<string>> GetAllAdminPhoneNumbers();
        public Task<IEnumerable<string>> GetAllAdminPhoneNumbers(Guid userId);
        public Task<IEnumerable<string>> GetAllAdminPhoneNumbersPerState(Guid stateId);
        public Task<IEnumerable<string>> GetAllAdminPhoneNumbers(Guid stateId, Guid lgaId);
        public Task<PaginatedList<AdminViewModel>> GetPaginatedListAsync(FilterOptions options, Guid userId);
        public Task<IEnumerable<AdminsPaymentViewModel>> filterAdminsPayment(AttendenceReportViewModel model);


    }
}
