using AutoMapper;
using MoneyBee.Transfer.Models;

namespace MoneyBee.Transfer.Mapping;

public class TransferMappingProfile : Profile
{
    public TransferMappingProfile()
    {
        CreateMap<Entities.Transfer, TransferResponse>();
    }
}
