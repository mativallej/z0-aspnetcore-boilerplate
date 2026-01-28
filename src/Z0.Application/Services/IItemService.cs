using Z0.Application.DTOs.Common;
using Z0.Application.DTOs.Items;

namespace Z0.Application.Services;

public interface IItemService
{
    Task<ItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ItemDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<ItemDto>> SearchAsync(string name, CancellationToken cancellationToken = default);
    Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken = default);
    Task<ItemDto> UpdateAsync(Guid id, UpdateItemRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
