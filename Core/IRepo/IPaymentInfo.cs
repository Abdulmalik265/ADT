using Core.Enum;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IRepo
{
    public interface IPaymentInfo : IBaseRepo<PaymentInfoViewModel>
    {
        public Task<PaginatedList<PaymentInfoViewModel>> GetPaginatedListAsync(FilterOptions options, Guid memberId);
        public Task<BaseResponse> CreateAsync(Month month, decimal amount, string createdBy, Guid memberId);

        public Task<BaseResponse> UpdateAsync(Guid memberId, Month month, decimal amount, string updatedBy);
    }
}
