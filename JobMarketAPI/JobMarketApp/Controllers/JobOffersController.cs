using JobMarketApp.API.DTO.JobOffers;
using JobMarketApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobMarketApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobOffersController : BaseController
    {
        private readonly IJobOfferService _jobOffersService;
        public JobOffersController(IJobOfferService jobOffersService)
        {
            _jobOffersService = jobOffersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var jobOffers = await _jobOffersService.GetAllAsync();
            return Ok(jobOffers);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var jobOffer = await _jobOffersService.GetByIdAsync(id);

            if (jobOffer is null)
                return NotFound();

            return Ok(jobOffer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateJobOfferDto jobOfferDto)
        {
            var created = await _jobOffersService.CreateAsync(jobOfferDto);

            return Ok(created);
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateJobOfferDto jobOfferDto)
        {
            var updated = await _jobOffersService.UpdateAsync(id, jobOfferDto);

            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var deleted = await _jobOffersService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptAsync([FromBody] AcceptJobOfferDto dto)
        {
            var result = await _jobOffersService.AcceptAsync(dto.JobId, dto.ContractorId);
            return Ok(result);
        }
    }
 }
