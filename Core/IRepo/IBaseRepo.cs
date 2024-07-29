using Core.Models;


namespace Core.IRepo
{
    public interface IBaseRepo<T>
    {
        public Task<BaseResponse> CreateAsync(T request, string createdBy);
        public Task<BaseResponse> UpdateAsync(T request, string updatedBy);
        public Task<BaseResponse> DeleteAsync(Guid id, string updatedBy);
        public Task<IEnumerable<T>> GetAllAsync();
        public Task<PaginatedList<T>> GetPaginatedListAsync(FilterOptions options);
        public Task<T?> GetByIdAsync(Guid id);
    }
}
