using AutoMapper;
using JobMarketApp.API.DTO.Jobs;
using JobMarketApp.Persistence.Models;

namespace JobMarketApp.API.Mappings
{
    public class JobProfile : Profile
    {
        public JobProfile()
        {
            CreateMap<Job, JobDto>().ReverseMap();

            CreateMap<CreateJobDto, Job>();
            CreateMap<UpdateJobDto, Job>();
        }
    }
}
