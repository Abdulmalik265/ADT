using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Services.Interface
{
    public interface ILocationService
    {
        public Task<IEnumerable<StateViewModel>> GetStatesAsync();
        public Task<IEnumerable<LocalGovernmentViewModel>> GetLocalGovernmentsAsync(Guid stateId);
        public Task<IEnumerable<string>> GetAllPhoneNumbersAsync();
        public Task<IEnumerable<string>> GetAllPhoneNumbersAsync(Guid userId);
        public Task<IEnumerable<string>> GetAllPhoneNumbersAsyncPerState(Guid stateId);
        public Task<IEnumerable<string>> GetAllPhoneNumbersAsync(Guid stateId, Guid lgaId);
        public Task<IEnumerable<string>> GetPhoneNumbersByStateAndLGAAsync(Guid stateId, Guid lgaId, string RecipientType);
        public Task<IEnumerable<string>> GetPhoneNumbersByStateAndLGAAsync(Guid stateId, string RecipientType);
        public Task<DashboardReportViewModel> GetDashboardReportAsync();
    }
}
