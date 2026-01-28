using AutoMapper;
using FluentValidation;
using Z0.Application.DTOs.Common;
using Z0.Application.DTOs.Items;
using Z0.Application.Exceptions;
using Z0.Application.Interfaces;
using Z0.Domain.Entities;

namespace Z0.Application.Services;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateItemRequest> _createValidator;
    private readonly IValidator<UpdateItemRequest> _updateValidator;

    public ItemService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateItemRequest> createValidator,
        IValidator<UpdateItemRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken);
        return item == null ? null : _mapper.Map<ItemDto>(item);
    }

    public async Task<IEnumerable<ItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Items.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ItemDto>>(items);
    }

    public async Task<PagedResult<ItemDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Items.GetPagedAsync(pageNumber, pageSize, cancellationToken);

        return new PagedResult<ItemDto>
        {
            Items = _mapper.Map<IEnumerable<ItemDto>>(items),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<ItemDto>> SearchAsync(string name, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Items.SearchByNameAsync(name, cancellationToken);
        return _mapper.Map<IEnumerable<ItemDto>>(items);
    }

    public async Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Exceptions.ValidationException(validationResult.Errors);
        }

        var item = new Item(request.Name, request.Description);
        await _unitOfWork.Items.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto> UpdateAsync(Guid id, UpdateItemRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Exceptions.ValidationException(validationResult.Errors);
        }

        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException(nameof(Item), id);
        }

        item.Update(request.Name, request.Description);
        await _unitOfWork.Items.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ItemDto>(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException(nameof(Item), id);
        }

        await _unitOfWork.Items.DeleteAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
