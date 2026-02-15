using AutoMapper;
using JobMarketApp.API.DTO.Contractors;
using JobMarketApp.Persistence.Models;

namespace JobMarketApp.API.Mappings
{
    public class ContractorProfile : Profile
    {
        public ContractorProfile()
        {
            CreateMap<Contractor, ContractorDto>().ReverseMap();

            //CreateMap<CreateContractorDto, Contractor>();
            //CreateMap<UpdateContractorDto, Contractor>();
        }
    }
}
