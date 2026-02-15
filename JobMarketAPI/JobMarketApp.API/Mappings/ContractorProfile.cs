using AutoMapper;
using JobMarketApp.API.DTO.Customers;
using JobMarketApp.Persistence.Models;

namespace JobMarketApp.API.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDto>().ReverseMap();

            //CreateMap<CreateCustomerDto, Customer>();
            //CreateMap<UpdateCustomerDto, Customer>();
        }
    }
}
