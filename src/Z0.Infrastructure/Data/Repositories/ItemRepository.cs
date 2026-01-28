using Microsoft.EntityFrameworkCore;
using Z0.Application.Interfaces;
using Z0.Domain.Entities;

namespace Z0.Infrastructure.Data.Repositories;

public class ItemRepository : BaseRepository<Item>, IItemRepository
{
    public ItemRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Item> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(x => x.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Item>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.IsActive && x.Name.ToLower().Contains(name.ToLower()))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
