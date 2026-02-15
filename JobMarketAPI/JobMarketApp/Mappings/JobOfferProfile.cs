using AutoMapper;
using JobMarketApp.API.DTO.JobOffers;
using JobMarketApp.Persistence.Models;

namespace JobMarketApp.API.Mappings
{
    public class JobOfferProfile : Profile
    {
        public JobOfferProfile()
        {
            CreateMap<JobOffer, JobOfferDto>().ReverseMap();

            CreateMap<CreateJobOfferDto, JobOffer>();
            CreateMap<UpdateJobOfferDto, JobOffer>();
        }
    }
}
