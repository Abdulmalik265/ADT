using Core.Enum;
using Core.IRepo;
using Core.Models;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repo
{
    public class PaymentRepo : IPaymentInfo
    {
        private readonly AdtDbContext _dbContext;
        public PaymentRepo(AdtDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<BaseResponse> CreateAsync(Month month, decimal amount, string createdBy, Guid memberId)
        {
            var check = await _dbContext.PaymentInfos.AnyAsync(x => x.MemberId == memberId && x.Month == month && x.IsPaid == true);
            if(check)
                return new BaseResponse() { Status = false, Message = "Payment of this member for this month exist!" };

            PaymentInfo paymentInfo = new PaymentInfo
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                IsPaid = true,
                MemberId = memberId,
                Created = DateTime.Now,
                Month = month
            };

            _dbContext.Add(paymentInfo);
            var status = await _dbContext.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "Member payment added successfully!" };
            return new BaseResponse() { Status = false, Message = "Server error!" };
        }

        public Task<BaseResponse> CreateAsync(PaymentInfoViewModel request, string createdBy)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> DeleteAsync(Guid id, string updatedBy)
        {
            var paymentInfo = await _dbContext.PaymentInfos.SingleOrDefaultAsync(x => x.Id == id);
            if (paymentInfo == null)
                return new BaseResponse();
            _dbContext.Remove(paymentInfo);
            var status = await _dbContext.TrySaveChangesAsync();
            if (status)
                return new BaseResponse() { Status = true, Message = "Member payment deleted successfully!" };
            return new BaseResponse() { Status = false, Message = "Server error!" };
        }

        public Task<IEnumerable<PaymentInfoViewModel>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<PaymentInfoViewModel?> GetByIdAsync(Guid id)
        {
            var paymentInfo = await _dbContext.PaymentInfos.Include(x => x.Member)
                .Select(x => new PaymentInfoViewModel
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    Month = x.Month,
                    Member = new MembersViewModel
                    {
                        Id = x.Member.Id,
                        FirstName = x.Member.FirstName,
                        SurName = x.Member.SurName,
                    }


                }).SingleOrDefaultAsync(x => x.Id == id);

            return paymentInfo;

        }

        public async Task<PaginatedList<PaymentInfoViewModel>> GetPaginatedListAsync(FilterOptions options)
        {
            var paymentsInDb = _dbContext.PaymentInfos.Select(x => new PaymentInfoViewModel
            {
                Amount = x.Amount,
                Month = x.Month,
                Id = x.Id,
                MemberId = x.MemberId

            }).AsNoTracking();

            var count = paymentsInDb.Count();
            var items = await paymentsInDb
                       .Skip((options.PageIndex - 1) * options.PageSize)
                       .Take(options.PageSize)
                       .ToListAsync();

            return PaginatedList<PaymentInfoViewModel>.Create(items, count, options);


        }

        public async Task<PaginatedList<PaymentInfoViewModel>> GetPaginatedListAsync(FilterOptions options, Guid memberId)
        {
            var member = await _dbContext.Members.SingleOrDefaultAsync(x => x.Id == memberId);
            var paymentsInDb = _dbContext.PaymentInfos.Include(x => x.Member)
                .Where(x => x.MemberId == memberId)
                .Select(x => new PaymentInfoViewModel
                {
                    Amount = x.Amount,
                    Month = x.Month,
                    Id = x.Id,
                    MemberId = x.MemberId,
                    Member = new MembersViewModel
                    {
                        Id = x.MemberId,
                        FirstName = x.Member.FirstName,
                        SurName = x.Member.SurName,
                        LocalGovernment = new LocalGovernmentViewModel
                        {
                            Name = x.Member.LocalGovernment.Name
                        }
                    }

                }).AsNoTracking();

            var count = paymentsInDb.Count();
            var items = await paymentsInDb
                       .Skip((options.PageIndex - 1) * options.PageSize)
                       .Take(options.PageSize)
                       .ToListAsync();

            return PaginatedList<PaymentInfoViewModel>.Create(items, count, options);

        }

        public async Task<BaseResponse> UpdateAsync(Guid paymentId, Month month, decimal amount, string updatedBy)
        {
            var check = await _dbContext.PaymentInfos.AnyAsync(x => x.Id == paymentId && x.Month != month && x.IsPaid == true);
            if (check)
                return new BaseResponse() { Status = false, Message = "Payment for this month exist!" };

            var paymentInfo = await _dbContext.PaymentInfos.SingleOrDefaultAsync(x => x.Id == paymentId);

            paymentInfo.Amount = amount;
            paymentInfo.Month = month;

            var status = await _dbContext.TrySaveChangesAsync();
            if (status)
                return new BaseResponse { Status = true, Message = "the payment updated successfully!" };

            return new BaseResponse { Status = false, Message = "Server error!" };
        }

        public Task<BaseResponse> UpdateAsync(PaymentInfoViewModel request, string updatedBy)
        {
            throw new NotImplementedException();
        }
    }
}
