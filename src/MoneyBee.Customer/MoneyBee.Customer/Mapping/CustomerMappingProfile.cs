using AutoMapper;
using MoneyBee.Customer.Models;

namespace MoneyBee.Customer.Mapping;

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Entities.Customer, CustomerResponse>();
        
        CreateMap<Entities.Customer, CustomerVerificationResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Name} {src.Surname}"))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == Shared.Models.CustomerStatus.Active));
    }
}
