using Z0.Domain.Entities;
using Z0.Domain.Interfaces;

namespace Z0.Application.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    Task<(IEnumerable<Item> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
}
