using AutoMapper;
using Z0.Application.DTOs.Items;
using Z0.Domain.Entities;

namespace Z0.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Item mappings
        CreateMap<Item, ItemDto>();

        CreateMap<CreateItemRequest, Item>()
            .ConstructUsing(src => new Item(src.Name, src.Description));
    }
}
