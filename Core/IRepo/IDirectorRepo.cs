using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IRepo
{
    public interface IDirectorRepo : IBaseRepo<DirectorViewModel>
    {
        public Task<DirectorViewModel> GetByUserIdAsync(Guid id);
        public Task<BaseResponse> UpdateProfile(string UserName, string Email, string Password, Guid id);
        public Task<IEnumerable<string>> GetAllDirectorPhoneNumbers();
        public Task<IEnumerable<string>> GetAllDirectorPhoneNumbers(Guid userId);
        public Task<IEnumerable<string>> GetAllDirectorPhoneNumbersPerState(Guid stateId);
        public Task<IEnumerable<DirectorsPaymentReportViewModel>> FilterDirectorsPayment(AttendenceReportViewModel model);


    }
}
