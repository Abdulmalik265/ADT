using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IRepo
{
    public interface IMembersRepo : IBaseRepo<MembersViewModel>
    {
        public Task<BaseResponse> UpdatePaymentInfoAsync(PaymentInfoViewModel model, Guid id);
        public Task<PaginatedList<MembersViewModel>> GetPaginatedListAsync(FilterOptions options, Guid userId);
        public Task<IEnumerable<string>> GetAllMembersPhoneNumber(Guid userId);
        public Task<IEnumerable<string>> GetAllMembersPhoneNumberPerState(Guid stateId);
        public Task<IEnumerable<string>> GetAllMembersPhoneNumber(Guid stateId, Guid lgaId);

        public Task<IEnumerable<string>> GetAllMembersPhoneNumber();
        public Task<IEnumerable<MembersViewModel>> filterMembers(AttendenceReportViewModel model);
        public Task<IEnumerable<MembersPaymentReportViewModel>> filterMembersPayment(AttendenceReportViewModel model);


    }
}
